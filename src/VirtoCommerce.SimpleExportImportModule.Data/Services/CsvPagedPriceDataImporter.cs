using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using FluentValidation;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Model.Search;
using VirtoCommerce.PricingModule.Core.Services;
using VirtoCommerce.SimpleExportImportModule.Core;
using VirtoCommerce.SimpleExportImportModule.Core.Models;
using VirtoCommerce.SimpleExportImportModule.Core.Services;
using VirtoCommerce.SimpleExportImportModule.Data.Validation;

namespace VirtoCommerce.SimpleExportImportModule.Data.Services
{
    public sealed class CsvPagedPriceDataImporter : ICsvPagedPriceDataImporter
    {
        private readonly IPricingService _pricingService;
        private readonly IPricingSearchService _pricingSearchService;
        private readonly ICsvPagedPriceDataSourceFactory _dataSourceFactory;
        private readonly IValidator<ImportProductPrice[]> _importProductPricesValidator;
        private readonly IBlobStorageProvider _blobStorageProvider;
        private readonly ICsvPriceDataValidator _csvPriceDataValidator;
        private readonly ICsvPriceImportReporterFactory _importReporterFactory;
        private readonly IBlobUrlResolver _blobUrlResolver;

        public CsvPagedPriceDataImporter(IBlobStorageProvider blobStorageProvider, IPricingService pricingService, IPricingSearchService pricingSearchService,
            ICsvPriceDataValidator csvPriceDataValidator, ICsvPagedPriceDataSourceFactory dataSourceFactory, IValidator<ImportProductPrice[]> importProductPricesValidator, ICsvPriceImportReporterFactory importReporterFactory, IBlobUrlResolver blobUrlResolver)
        {
            _pricingService = pricingService;
            _pricingSearchService = pricingSearchService;
            _dataSourceFactory = dataSourceFactory;
            _importProductPricesValidator = importProductPricesValidator;
            _blobStorageProvider = blobStorageProvider;
            _csvPriceDataValidator = csvPriceDataValidator;
            _importReporterFactory = importReporterFactory;
            _blobUrlResolver = blobUrlResolver;
        }

        public async Task ImportAsync(ImportDataRequest request, Action<ImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {

            ValidateParameters(request, progressCallback, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            var errorsContext = new ImportErrorsContext();

            var csvPriceDataValidationResult = await _csvPriceDataValidator.ValidateAsync(request.FileUrl);

            if (csvPriceDataValidationResult.Errors.Any())
            {
                throw new InvalidDataException();
            }

            await using var stream = _blobStorageProvider.OpenRead(request.FileUrl);

            var reportFileUrl = GetReportFileUrl(request.FileUrl);

            await using var importReporterStream = _blobStorageProvider.OpenWrite(reportFileUrl);

            var csvConfiguration = new ImportConfiguration();

            var importReporter = _importReporterFactory.Create(importReporterStream, csvConfiguration);

            cancellationToken.ThrowIfCancellationRequested();

            var importProgress = new ImportProgressInfo { ProcessedCount = 0, CreatedCount = 0, UpdatedCount = 0, Description = "Import has started" };

            var configuration = new ImportConfiguration();

            var dataSource = _dataSourceFactory.Create(stream, ModuleConstants.Settings.PageSize, configuration);

            var headerRaw = dataSource.GetHeaderRaw();

            if (!headerRaw.IsNullOrEmpty())
            {
                importReporter.WriteHeader(headerRaw);
            }

            importProgress.TotalCount = dataSource.GetTotalCount();
            progressCallback(importProgress);

            const string importDescription = "{0} out of {1} have been imported.";

            configuration.ReadingExceptionOccurred = exception =>
            {
                var context = exception.ReadingContext;

                if (!errorsContext.ErrorsRows.Contains(context.Row))
                {
                    var fieldSourceValue = context.Record[context.CurrentIndex];
                    if (context.HeaderRecord.Length != context.Record.Length)
                    {
                        HandleNotClosedQuoteError(progressCallback, importProgress, importReporter, context, errorsContext);
                    }
                    else if (fieldSourceValue == "")
                    {
                        HandleRequiredValueError(progressCallback, importProgress, importReporter, context, errorsContext);
                    }
                    else
                    {
                        HandleWrongValueError(progressCallback, importProgress, importReporter, context, errorsContext);
                    }
                }

                return false;
            };

            configuration.BadDataFound = async context =>
            {
                await HandleBadDataError(progressCallback, importProgress, importReporter, context, errorsContext);
            };

            configuration.MissingFieldFound = async (headerNames, index, context) =>
                await HandleMissedColumnError(progressCallback, importProgress, importReporter, context, errorsContext, headerNames);

            try
            {
                importProgress.Description = "Fetching...";
                progressCallback(importProgress);

                var importProductPricesExistValidator = new ImportProductPricesExistenceValidator(_pricingSearchService, ImportProductPricesExistenceValidationMode.Exists);

                while (await dataSource.FetchAsync())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var importProductPrices = dataSource.Items
                        // expect records that was parsed with errors
                        .Where(importProductPrice => !errorsContext.ErrorsRows.Contains(importProductPrice.Row))
                        .Select(importProductPrice =>
                    {
                        importProductPrice.Price.PricelistId = request.PricelistId;
                        return importProductPrice;
                    }).ToArray();

                    try
                    {
                        var createdPrices = new List<Price>();
                        var updatedPrices = new List<Price>();

                        var validationResult = await _importProductPricesValidator.ValidateAsync(importProductPrices, ruleSet: request.ImportMode.ToString());

                        var invalidImportProductPrices = validationResult.Errors.Select(x => (x.CustomState as ImportValidationState)?.InvalidImportProductPrice).Distinct().ToArray();

                        var errorsInfos = validationResult.Errors.Select(x => new { Message = x.ErrorMessage, ImportProductPrice = (x.CustomState as ImportValidationState)?.InvalidImportProductPrice }).ToArray();

                        var errorsGroups = errorsInfos.GroupBy(x => x.ImportProductPrice);

                        foreach (var group in errorsGroups)
                        {
                            var importPrice = group.Key;

                            var errorMessages = string.Join(" ", group.Select(x => x.Message).ToArray());

                            var importError = new ImportError { Error = errorMessages, RawRow = importPrice.RawRecord };

                            await importReporter.WriteAsync(importError);
                        }

                        importProgress.ErrorCount += invalidImportProductPrices.Length;
                        importProductPrices = importProductPrices.Except(invalidImportProductPrices).ToArray();

                        switch (request.ImportMode)
                        {
                            case ImportMode.CreateOnly:
                                createdPrices.AddRange(importProductPrices.Select(importProductPrice => importProductPrice.Price));
                                break;
                            case ImportMode.UpdateOnly:
                                var existingPrices = await GetAndPatchExistingPrices(request, importProductPrices);
                                updatedPrices.AddRange(existingPrices);
                                break;
                            case ImportMode.CreateAndUpdate:
                                var importProductPriceNotExistValidationResult = await importProductPricesExistValidator.ValidateAsync(importProductPrices);

                                var importProductPricesToCreate = importProductPriceNotExistValidationResult.Errors
                                    .Select(x => (x.CustomState as ImportValidationState)?.InvalidImportProductPrice).Distinct().ToArray();

                                var importProductPricesToUpdate = importProductPrices.Except(importProductPricesToCreate).ToArray();
                                createdPrices.AddRange(importProductPricesToCreate.Select(importProductPrice => importProductPrice.Price));
                                var existingPricesToUpdate = await GetAndPatchExistingPrices(request, importProductPricesToUpdate);
                                updatedPrices.AddRange(existingPricesToUpdate);
                                break;
                            default:
                                throw new ArgumentException("Import mode has invalid value", nameof(request));
                        }

                        var allChangingPrices = createdPrices.Concat(updatedPrices).ToArray();
                        await _pricingService.SavePricesAsync(allChangingPrices);

                        importProgress.CreatedCount += createdPrices.Count;
                        importProgress.UpdatedCount += updatedPrices.Count;
                    }
                    catch (Exception e)
                    {
                        HandleError(progressCallback, importProgress, e.Message);
                    }
                    finally
                    {
                        importProgress.ProcessedCount = Math.Min(dataSource.CurrentPageNumber * dataSource.PageSize, importProgress.TotalCount);
                    }

                    if (importProgress.ProcessedCount != importProgress.TotalCount)
                    {
                        importProgress.Description = string.Format(importDescription, importProgress.ProcessedCount, importProgress.TotalCount);
                        progressCallback(importProgress);
                    }
                }
            }
            catch (Exception e)
            {
                HandleError(progressCallback, importProgress, e.Message);
            }
            finally
            {
                var completedMessage = importProgress.ErrorCount > 0 ? "Import completed with errors" : "Import completed";
                importProgress.Description = $"{completedMessage}: {string.Format(importDescription, importProgress.ProcessedCount, importProgress.TotalCount)}";

                // Need to dispose importer manually, because we need flush buffer and potentially remove empty report
                importReporter.Dispose();

                var reportFileSize = (await _blobStorageProvider.GetBlobInfoAsync(reportFileUrl)).Size;

                if (reportFileSize > 0)
                {
                    importProgress.ReportUrl = _blobUrlResolver.GetAbsoluteUrl(reportFileUrl);
                }
                else
                {
                    await _blobStorageProvider.RemoveAsync(new[] { reportFileUrl });
                }

                progressCallback(importProgress);

            }
        }

        private async Task<Price[]> GetAndPatchExistingPrices(ImportDataRequest request, ImportProductPrice[] importProductPrices)
        {
            var updateProductIds = importProductPrices.Select(x => x.ProductId).ToArray();
            var existingPricesSearchResult = await _pricingSearchService.SearchPricesAsync(
                new PricesSearchCriteria { ProductIds = updateProductIds, PriceListIds = new[] { request.PricelistId }, Take = int.MaxValue });
            var existingPrices = existingPricesSearchResult
                .Results
                .Where(x => importProductPrices.Select(i => i.Price).Any(p => p.MinQuantity == x.MinQuantity && p.ProductId == x.ProductId))
                .ToArray();

            foreach (var existingPrice in existingPrices)
            {
                var priceForUpdating = importProductPrices
                    .FirstOrDefault(x =>
                        x.ProductId == existingPrice.ProductId &&
                        x.Price.MinQuantity == existingPrice.MinQuantity);
                if (priceForUpdating != null)
                {
                    existingPrice.Sale = priceForUpdating.Price.Sale;
                    existingPrice.List = priceForUpdating.Price.List;
                }
            }

            return existingPrices;
        }

        private static string GetReportFileUrl(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            var fileExtension = Path.GetExtension(fileName);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            var reportFileName = $"{fileNameWithoutExtension}_report{fileExtension}";
            var result = filePath.Replace(fileName, reportFileName);

            return result;
        }

        private static void ValidateParameters(ImportDataRequest request, Action<ImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (progressCallback == null)
            {
                throw new ArgumentNullException(nameof(progressCallback));
            }

            if (cancellationToken == null)
            {
                throw new ArgumentNullException(nameof(cancellationToken));
            }
        }

        private static void HandleError(Action<ImportProgressInfo> progressCallback, ImportProgressInfo importProgress, string error = null)
        {
            if (error != null)
            {
                importProgress.Errors.Add(error);
            }

            importProgress.ErrorCount++;
            progressCallback(importProgress);
        }

        private static async Task HandleBadDataError(Action<ImportProgressInfo> progressCallback, ImportProgressInfo importProgress, ICsvPriceImportReporter reporter, ReadingContext context, ImportErrorsContext errorsContext)
        {
            var importError = new ImportError { Error = "This row has invalid data. The data after field with not escaped quote was lost.", RawRow = context.RawRecord };

            await reporter.WriteAsync(importError);

            errorsContext.ErrorsRows.Add(context.Row);
            HandleError(progressCallback, importProgress);
        }

        private static void HandleNotClosedQuoteError(Action<ImportProgressInfo> progressCallback, ImportProgressInfo importProgress, ICsvPriceImportReporter reporter, ReadingContext context, ImportErrorsContext errorsContext)
        {
            var importError = new ImportError { Error = "This row has invalid data. Quotes should be closed.", RawRow = context.RawRecord };

            reporter.Write(importError);

            errorsContext.ErrorsRows.Add(context.Row);
            HandleError(progressCallback, importProgress);
        }

        private static void HandleWrongValueError(Action<ImportProgressInfo> progressCallback, ImportProgressInfo importProgress, ICsvPriceImportReporter reporter, ReadingContext context, ImportErrorsContext errorsContext)
        {
            var invalidFieldName = context.HeaderRecord[context.CurrentIndex];
            var importError = new ImportError { Error = $"This row has invalid value in the column {invalidFieldName}.", RawRow = context.RawRecord };

            reporter.Write(importError);

            errorsContext.ErrorsRows.Add(context.Row);
            HandleError(progressCallback, importProgress);
        }

        private static void HandleRequiredValueError(Action<ImportProgressInfo> progressCallback, ImportProgressInfo importProgress, ICsvPriceImportReporter reporter, ReadingContext context, ImportErrorsContext errorsContext)
        {
            var fieldName = context.HeaderRecord[context.CurrentIndex];

            var requiredFields = CsvPriceImportHelper.GetImportPriceRequiredValueColumns();

            var missedValueColumns = new List<string>();

            for (var i = 0; i < context.HeaderRecord.Length; i++)
            {
                if (requiredFields.Contains(context.HeaderRecord[i], StringComparer.InvariantCultureIgnoreCase) && context.Record[i].IsNullOrEmpty())
                {
                    missedValueColumns.Add(context.HeaderRecord[i]);
                }
            }

            var importError = new ImportError { Error = $"The required value in column {fieldName} is missing.", RawRow = context.RawRecord };

            if (missedValueColumns.Count > 1)
            {
                importError.Error = $"The required values in columns: {string.Join(", ", missedValueColumns)} - are missing.";
            }

            reporter.Write(importError);

            errorsContext.ErrorsRows.Add(context.Row);
            HandleError(progressCallback, importProgress);
        }

        private static async Task HandleMissedColumnError(Action<ImportProgressInfo> progressCallback, ImportProgressInfo importProgress, ICsvPriceImportReporter reporter, ReadingContext context, ImportErrorsContext errorsContext, string[] headerNames)
        {
            var headerColumns = context.HeaderRecord;

            var recordFields = context.Record;

            var missedColumns = headerColumns.Skip(recordFields.Length).ToArray();

            var error = $"This row has next missing columns: {string.Join(", ", missedColumns)}.";

            var importError = new ImportError { Error = error, RawRow = context.RawRecord };

            await reporter.WriteAsync(importError);

            errorsContext.ErrorsRows.Add(context.Row);
            HandleError(progressCallback, importProgress);
        }
    }
}
