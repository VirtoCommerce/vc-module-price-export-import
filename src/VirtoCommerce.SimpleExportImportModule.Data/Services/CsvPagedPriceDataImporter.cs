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

        public CsvPagedPriceDataImporter(IBlobStorageProvider blobStorageProvider, IPricingService pricingService, IPricingSearchService pricingSearchService,
            ICsvPriceDataValidator csvPriceDataValidator, ICsvPagedPriceDataSourceFactory dataSourceFactory, IValidator<ImportProductPrice[]> importProductPricesValidator)
        {
            _pricingService = pricingService;
            _pricingSearchService = pricingSearchService;
            _dataSourceFactory = dataSourceFactory;
            _importProductPricesValidator = importProductPricesValidator;
            _blobStorageProvider = blobStorageProvider;
            _csvPriceDataValidator = csvPriceDataValidator;
        }

        public async Task ImportAsync(ImportDataRequest request, Action<ImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {

            ValidateParameters(request, progressCallback, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            var csvPriceDataValidationResult = await _csvPriceDataValidator.ValidateAsync(request.FileUrl);

            if (csvPriceDataValidationResult.Errors.Any())
            {
                throw new InvalidDataException();
            }

            await using var stream = _blobStorageProvider.OpenRead(request.FileUrl);

            await using var importReporterStream = _blobStorageProvider.OpenWrite(GetReportFileName(request.FileUrl));

            var csvConfiguration = new ImportConfiguration();

            var importReporter = new CsvPriceImportReporter(importReporterStream, csvConfiguration);

            cancellationToken.ThrowIfCancellationRequested();
            
            var importProgress = new ImportProgressInfo { ProcessedCount = 0, CreatedCount = 0, UpdatedCount = 0, Description = "Import has started" };

            var dataSource = _dataSourceFactory.Create(stream, ModuleConstants.Settings.PageSize, new ImportConfiguration
            {
                ReadingExceptionOccurred = exception =>
                {
                    HandleError(progressCallback, importProgress);
                    return false;
                },
                BadDataFound = context => HandleBadDataError(progressCallback, importProgress, importReporter, context),
                MissingFieldFound = (headerNames, index, context) => HandleMissedColumnError(progressCallback, importProgress, importReporter, context, headerNames)
            });

            importProgress.TotalCount = dataSource.GetTotalCount();
            progressCallback(importProgress);

            const string importDescription = "{0} out of {1} have been imported.";

            try
            {
                importProgress.Description = "Fetching...";
                progressCallback(importProgress);

                var importProductPricesNotExistValidator = new ImportProductPricesExistenceValidator(_pricingSearchService, ImportProductPricesExistenceValidationMode.NotExists);

                while (await dataSource.FetchAsync())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var importProductPrices = dataSource.Items.Select(importProductPrice =>
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
                        importProgress.ErrorCount += invalidImportProductPrices.Length;
                        importProductPrices = importProductPrices.Except(invalidImportProductPrices).ToArray();

                        switch (request.ImportMode)
                        {
                            case ImportMode.CreateOnly:
                                createdPrices.AddRange(importProductPrices.Select(importProductPrice => importProductPrice.Price));
                                break;
                            case ImportMode.UpdateOnly:
                                updatedPrices.AddRange(importProductPrices.Select(importProductPrice => importProductPrice.Price));
                                break;
                            case ImportMode.CreateAndUpdate:
                                var importProductPriceNotExistValidationResult = await importProductPricesNotExistValidator.ValidateAsync(importProductPrices);
                                var importProductPricesToCreate = importProductPriceNotExistValidationResult.Errors.Select(x => (x.CustomState as ImportValidationState)?.InvalidImportProductPrice).Distinct().ToArray();
                                var importProductPricesToUpdate = importProductPrices.Except(importProductPricesToCreate).ToArray();
                                createdPrices.AddRange(importProductPricesToCreate.Select(importProductPrice => importProductPrice.Price));
                                updatedPrices.AddRange(importProductPricesToUpdate.Select(importProductPrice => importProductPrice.Price));
                                break;
                            default:
                                throw new ArgumentException("Import mode has invalid value", nameof(request));
                        }

                        await _pricingService.SavePricesAsync(createdPrices.Concat(updatedPrices).ToArray());

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
                progressCallback(importProgress);

            }
        }

        private static string GetReportFileName(string fileUrl)
        {
            var uri = new Uri(fileUrl);
            var fileName = uri.Segments.Last();
            var fileExtension = Path.GetExtension(fileName);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            var reportFileName = $"{fileNameWithoutExtension}_report.{fileExtension}";
            var result = fileUrl.Replace(fileName, reportFileName);

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

        private static async void HandleBadDataError(Action<ImportProgressInfo> progressCallback, ImportProgressInfo importProgress, CsvPriceImportReporter reporter, ReadingContext context)
        {
            var importError = new ImportError { Error = "This row has invalid data", RawRow = context.RawRecord };
            await reporter.WriteAsync(importError);
            HandleError(progressCallback, importProgress);
        }

        private static async void HandleMissedColumnError(Action<ImportProgressInfo> progressCallback, ImportProgressInfo importProgress, CsvPriceImportReporter reporter, ReadingContext context, string[] headerNames)
        {
            string error;

            if (headerNames.Length == 1)
            {
                error = $"Column {headerNames.First()} is required";
            } else
            {
                error = $"Columns {String.Join(',', headerNames)} are required";
            }

            var importError = new ImportError { Error = error, RawRow = context.RawRecord };
            await reporter.WriteAsync(importError);
            HandleError(progressCallback, importProgress);
        }
    }
}
