using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Services;
using VirtoCommerce.SimpleExportImportModule.Core;
using VirtoCommerce.SimpleExportImportModule.Core.Models;
using VirtoCommerce.SimpleExportImportModule.Core.Services;

namespace VirtoCommerce.SimpleExportImportModule.Data.Services
{
    public sealed class CsvPagedPriceDataImporter: ICsvPagedPriceDataImporter
    {
        private readonly IPricingService _pricingService;
        private readonly ICsvPagedPriceDataSourceFactory _dataSourceFactory;
        private readonly IValidator<ImportProductPrice[]> _importProductPricesValidator;

        public CsvPagedPriceDataImporter(IPricingService pricingService, ICsvPagedPriceDataSourceFactory dataSourceFactory, IValidator<ImportProductPrice[]> importProductPricesValidator)
        {
            _pricingService = pricingService;
            _dataSourceFactory = dataSourceFactory;
            _importProductPricesValidator = importProductPricesValidator;
        }

        public async Task ImportAsync(Stream stream, ImportDataRequest request, Action<ImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {

            ValidateParameters(stream, request, progressCallback, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            // TODO: Run import file validation

            cancellationToken.ThrowIfCancellationRequested();

            var importProgress = new ImportProgressInfo { ProcessedCount = 0, CreatedCount = 0, UpdatedCount = 0, Description = "Import has started" };

            var dataSource = _dataSourceFactory.Create(stream, ModuleConstants.Settings.PageSize, new ImportConfiguration
            {
                BadDataFound = context => HandleError(progressCallback, importProgress),
                MissingFieldFound = (headerNames, index, context) => HandleError(progressCallback, importProgress)
            });

            importProgress.TotalCount = dataSource.GetTotalCount();
            progressCallback(importProgress);

            const string importDescription = "{0} out of {1} have been imported.";

            try
            {
                importProgress.Description = "Fetching...";
                progressCallback(importProgress);

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

                        switch (request.ImportMode)
                        {
                            case ImportMode.CreateAndUpdate:
                                throw new NotImplementedException();
                            case ImportMode.CreateOnly:
                                var validationResult = await _importProductPricesValidator.ValidateAsync(importProductPrices);
                                var invalidProducts = validationResult.Errors.Select(x => (x.CustomState as ImportValidationState)?.InvalidImportProductPrice).Distinct().ToArray();
                                importProgress.ErrorCount += invalidProducts.Length;
                                importProductPrices = importProductPrices.Except(invalidProducts).ToArray();
                                createdPrices.AddRange(importProductPrices.Select(importProductPrice => importProductPrice.Price));
                                break;
                            case ImportMode.UpdateOnly:
                                throw new NotImplementedException();
                            default:
                                throw new ArgumentException("Import mode has invalid value", nameof(request));
                        }

                        await _pricingService.SavePricesAsync(createdPrices.ToArray());

                        importProgress.CreatedCount += createdPrices.Count;
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

        private static void ValidateParameters(Stream stream, ImportDataRequest request, Action<ImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
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
    }
}
