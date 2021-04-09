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
    public class ImportProductPricesExistenceValidator: AbstractValidator<ImportProductPrice[]>
    {
        public const string ExistingPrices = nameof(ExistingPrices);

        private readonly IPricingSearchService _pricingSearchService;
        private readonly bool _not;

        public ImportProductPricesExistenceValidator(IPricingSearchService pricingSearchService, bool not = false)
        {
            _pricingSearchService = pricingSearchService;
            _not = not;
            AttachValidators();
        }
        public void AttachValidators()
        {
            RuleFor(importProductPrices => importProductPrices)
                .CustomAsync(LoadExistingPricesAsync)
                .ForEach(rule => rule.SetValidator(_ => _not ? (AbstractValidator<ImportProductPrice>) new ImportProductPriceNotExistsValidator() : new ImportProductPriceExistsValidator()));
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
