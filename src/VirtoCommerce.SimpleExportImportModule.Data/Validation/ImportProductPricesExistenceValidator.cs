using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Validators;
using VirtoCommerce.PricingModule.Core.Model.Search;
using VirtoCommerce.PricingModule.Core.Services;
using VirtoCommerce.SimpleExportImportModule.Core.Models;

namespace VirtoCommerce.SimpleExportImportModule.Data.Validation
{
    public sealed class ImportProductPricesExistenceValidator : AbstractValidator<ImportProductPrice[]>
    {
        internal const string ExistingPrices = nameof(ExistingPrices);

        private readonly IPricingSearchService _pricingSearchService;
        private readonly ImportProductPricesExistenceValidationMode _mode;

        public ImportProductPricesExistenceValidator(IPricingSearchService pricingSearchService, ImportProductPricesExistenceValidationMode mode)
        {
            _pricingSearchService = pricingSearchService;
            _mode = mode;
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(importProductPrices => importProductPrices)
                .CustomAsync(LoadExistingPricesAsync)
                .ForEach(rule => rule.SetValidator(_ =>
                    _mode == ImportProductPricesExistenceValidationMode.NotExists
                        ? (AbstractValidator<ImportProductPrice>) new ImportProductPriceNotExistsValidator()
                        : new ImportProductPriceExistsValidator()));
        }

        private async Task LoadExistingPricesAsync(ImportProductPrice[] importProductPrices, CustomContext context, CancellationToken cancellationToken)
        {
            var productIds = importProductPrices.Select(importProductPrice => importProductPrice.ProductId).ToArray();
            var priceListIds = importProductPrices.Select(importProductPrice => importProductPrice.Price.PricelistId).ToArray();
            var existingPricesSearchResult = await _pricingSearchService.SearchPricesAsync(new PricesSearchCriteria { ProductIds = productIds, PriceListIds = priceListIds });
            var existingPrices = existingPricesSearchResult.Results.ToArray();
            context.ParentContext.RootContextData[ExistingPrices] = existingPrices;
        }
    }
}
