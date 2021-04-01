using System.Linq;
using FluentValidation;
using VirtoCommerce.PricingModule.Core.Model.Search;
using VirtoCommerce.PricingModule.Core.Services;
using VirtoCommerce.SimpleExportImportModule.Core.Models;

namespace VirtoCommerce.SimpleExportImportModule.Data.Validation
{
    public sealed class ImportProductPricesValidator: AbstractValidator<ImportProductPrice[]>
    {
        private readonly IPricingSearchService _pricingSearchService;

        public ImportProductPricesValidator(IPricingSearchService pricingSearchService)
        {
            _pricingSearchService = pricingSearchService;
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleForEach(importProductPrices => importProductPrices).SetValidator(importProductPrices =>
            {
                var duplicates = importProductPrices
                    .GroupBy(importProductPrice => new { importProductPrice.Sku, importProductPrice.Price.MinQuantity })
                    .SelectMany(group => group.Skip(1));
                return new ImportProductPriceIsNotDuplicateValidator(duplicates);
            });
            RuleForEach(importProductPrices => importProductPrices).SetValidator(importProductPrices =>
            {
                var productIds = importProductPrices.Select(importProductPrice => importProductPrice.ProductId).ToArray();
                var pricelistIds = importProductPrices.Select(importProductPrice => importProductPrice.Price.PricelistId).ToArray();
                var existingPrices = _pricingSearchService.SearchPricesAsync(new PricesSearchCriteria
                {
                    ProductIds = productIds,
                    PriceListIds = pricelistIds
                }).GetAwaiter().GetResult().Results;
                return new ImportProductPriceNotExistsValidator(existingPrices);
            });
            RuleForEach(importProductPrices => importProductPrices).SetValidator(importProductPrice => new ImportProductPriceProductExistsValidator());
        }
    }
}
