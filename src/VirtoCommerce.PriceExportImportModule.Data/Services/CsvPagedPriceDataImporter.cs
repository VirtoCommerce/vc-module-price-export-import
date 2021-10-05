using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using FluentValidation;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PriceExportImportModule.Core;
using VirtoCommerce.PriceExportImportModule.Core.Models;
using VirtoCommerce.PriceExportImportModule.Core.Services;
using VirtoCommerce.PriceExportImportModule.Data.Validation;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Model.Search;
using VirtoCommerce.PricingModule.Core.Services;

namespace VirtoCommerce.PriceExportImportModule.Data.Services
{
    public sealed class CsvPagedPriceDataImporter : ICsvPagedPriceDataImporter
    {
        private readonly IPricingService _pricingService;
        private readonly IPricingSearchService _pricingSearchService;
        private readonly ICsvPagedPriceDataSourceFactory _dataSourceFactory;
        private readonly IValidator<ImportProductPrice[]> _importProductPricesValidator;
        private readonly ICsvPriceDataValidator _csvPriceDataValidator;
        private readonly ICsvPriceImportReporterFactory _importReporterFactory;
        private readonly IBlobUrlResolver _blobUrlResolver;

        public CsvPagedPriceDataImporter(IBlobStorageProvider blobStorageProvider, IPricingService pricingService, IPricingSearchService pricingSearchService,
            ICsvPriceDataValidator csvPriceDataValidator, ICsvPagedPriceDataSourceFactory dataSourceFactory, IValidator<ImportProductPrice[]> importProductPricesValidator, ICsvPriceImportReporterFactory importReporterFactory
            , IBlobUrlResolver blobUrlResolver)
        {
            _pricingService = pricingService;
            _pricingSearchService = pricingSearchService;
            _dataSourceFactory = dataSourceFactory;
            _importProductPricesValidator = importProductPricesValidator;
            _csvPriceDataValidator = csvPriceDataValidator;
            _importReporterFactory = importReporterFactory;
            _blobUrlResolver = blobUrlResolver;
        }

        public async Task ImportAsync(ImportDataRequest request, Action<ImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {

            ValidateParameters(request, progressCallback, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            var errorsContext = new ImportErrorsContext();

            var csvPriceDataValidationResult = await _csvPriceDataValidator.ValidateAsync(request.FilePath);

            if (csvPriceDataValidationResult.Errors.Any())
            {
                throw new InvalidDataException();
            }

            var reportFilePath = GetReportFilePath(request.FilePath);

            var configuration = ImportConfiguration.GetCsvConfiguration();

            await using var importReporter = await _importReporterFactory.CreateAsync(reportFilePath, configuration.Delimiter);

            cancellationToken.ThrowIfCancellationRequested();

            var importProgress = new ImportProgressInfo { ProcessedCount = 0, CreatedCount = 0, UpdatedCount = 0, Description = "Import has started" };


            using var dataSource = _dataSourceFactory.Create(request.FilePath, ModuleConstants.Settings.PageSize, configuration);

            var headerRaw = dataSource.GetHeaderRaw();

            if (!headerRaw.IsNullOrEmpty())
            {
                importReporter.WriteHeader(headerRaw);
            }

            importProgress.TotalCount = dataSource.GetTotalCount();
            progressCallback(importProgress);

            const string importDescription = "{0} out of {1} have been imported.";

            configuration.ReadingExceptionOccurred = args =>
            {
                var context = args.Exception.Context;

                if (!errorsContext.ErrorsRows.Contains(context.Parser.Row))
                {
                    var fieldSourceValue = context.Reader[context.Reader.CurrentIndex];

                    if (context.Reader.HeaderRecord.Length != context.Parser.Record.Length)
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

            configuration.BadDataFound = args => HandleBadDataError(progressCallback, importProgress, importReporter, args.Context, errorsContext);

            configuration.MissingFieldFound = args => HandleMissedColumnError(progressCallback, importProgress, importReporter, args.Context, errorsContext);

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

                if (importReporter.ReportIsNotEmpty)
                {
                    importProgress.ReportUrl = _blobUrlResolver.GetAbsoluteUrl(reportFilePath);
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

        private static string GetReportFilePath(string filePath)
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

        private static void HandleBadDataError(Action<ImportProgressInfo> progressCallback, ImportProgressInfo importProgress, ICsvPriceImportReporter reporter, CsvContext context, ImportErrorsContext errorsContext)
        {
            var importError = new ImportError { Error = "This row has invalid data. The data after field with not escaped quote was lost.", RawRow = context.Parser.RawRecord };

            reporter.Write(importError);

            errorsContext.ErrorsRows.Add(context.Parser.Row);
            HandleError(progressCallback, importProgress);
        }

        private static void HandleNotClosedQuoteError(Action<ImportProgressInfo> progressCallback, ImportProgressInfo importProgress, ICsvPriceImportReporter reporter, CsvContext context, ImportErrorsContext errorsContext)
        {
            var importError = new ImportError { Error = "This row has invalid data. Quotes should be closed.", RawRow = context.Parser.RawRecord };

            reporter.Write(importError);

            errorsContext.ErrorsRows.Add(context.Parser.Row);
            HandleError(progressCallback, importProgress);
        }

        private static void HandleWrongValueError(Action<ImportProgressInfo> progressCallback, ImportProgressInfo importProgress, ICsvPriceImportReporter reporter, CsvContext context, ImportErrorsContext errorsContext)
        {
            var invalidFieldName = context.Reader.HeaderRecord[context.Reader.CurrentIndex];
            var importError = new ImportError { Error = $"This row has invalid value in the column {invalidFieldName}.", RawRow = context.Parser.RawRecord };

            reporter.Write(importError);

            errorsContext.ErrorsRows.Add(context.Parser.Row);
            HandleError(progressCallback, importProgress);
        }

        private static void HandleRequiredValueError(Action<ImportProgressInfo> progressCallback, ImportProgressInfo importProgress, ICsvPriceImportReporter reporter, CsvContext context, ImportErrorsContext errorsContext)
        {
            var fieldName = context.Reader.HeaderRecord[context.Reader.CurrentIndex];
            var requiredFields = CsvPriceImportHelper.GetImportPriceRequiredValueColumns();
            var missedValueColumns = new List<string>();

            for (var i = 0; i < context.Reader.HeaderRecord.Length; i++)
            {
                if (requiredFields.Contains(context.Reader.HeaderRecord[i], StringComparer.InvariantCultureIgnoreCase) && context.Parser.Record[i].IsNullOrEmpty())
                {
                    missedValueColumns.Add(context.Reader.HeaderRecord[i]);
                }
            }

            var importError = new ImportError { Error = $"The required value in column {fieldName} is missing.", RawRow = context.Parser.RawRecord };

            if (missedValueColumns.Count > 1)
            {
                importError.Error = $"The required values in columns: {string.Join(", ", missedValueColumns)} - are missing.";
            }

            reporter.Write(importError);

            errorsContext.ErrorsRows.Add(context.Parser.Row);
            HandleError(progressCallback, importProgress);
        }

        private static void HandleMissedColumnError(Action<ImportProgressInfo> progressCallback, ImportProgressInfo importProgress, ICsvPriceImportReporter reporter, CsvContext context, ImportErrorsContext errorsContext)
        {
            var headerColumns = context.Reader.HeaderRecord;
            var recordFields = context.Parser.Record;
            var missedColumns = headerColumns.Skip(recordFields.Length).ToArray();
            var error = $"This row has next missing columns: {string.Join(", ", missedColumns)}.";
            var importError = new ImportError { Error = error, RawRow = context.Parser.RawRecord };

            reporter.Write(importError);

            errorsContext.ErrorsRows.Add(context.Parser.Row);
            HandleError(progressCallback, importProgress);
        }
    }
}
