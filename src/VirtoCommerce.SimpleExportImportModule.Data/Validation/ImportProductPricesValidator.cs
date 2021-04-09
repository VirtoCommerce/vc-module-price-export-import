using FluentValidation;
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
            RuleSet(nameof(ImportMode.CreateOnly), () =>
            {
                RuleFor(importProductPrices => importProductPrices).SetValidator(_ => new ImportProductPricesAreNotDuplicatesValidator(ImportMode.CreateOnly), "default");
                RuleFor(importProductPrices => importProductPrices).SetValidator(_ => new ImportProductPricesExistenceValidator(_pricingSearchService, true), "default");
            });
            RuleSet(nameof(ImportMode.UpdateOnly), () =>
            {
                RuleFor(importProductPrices => importProductPrices).SetValidator(_ => new ImportProductPricesAreNotDuplicatesValidator(ImportMode.UpdateOnly), "default");
                RuleFor(importProductPrices => importProductPrices).SetValidator(_ => new ImportProductPricesExistenceValidator(_pricingSearchService), "default");
            });
            RuleSet(nameof(ImportMode.CreateAndUpdate), () =>
            {
                RuleFor(importProductPrices => importProductPrices).SetValidator(_ => new ImportProductPricesAreNotDuplicatesValidator(ImportMode.CreateAndUpdate), "default");
            });
            RuleSet($"{nameof(ImportMode.CreateOnly)},{nameof(ImportMode.UpdateOnly)},{nameof(ImportMode.CreateAndUpdate)}", () =>
            {
                RuleForEach(importProductPrices => importProductPrices).SetValidator(importProductPrice => new ImportProductPriceProductExistsValidator(), "default");
                RuleForEach(importProductPrice => importProductPrice).ChildRules(validator =>
                {
                    validator.RuleFor(importProductPrice => importProductPrice.Price.MinQuantity).GreaterThan(0);
                    validator.RuleFor(importProductPrice => importProductPrice.Price.List).GreaterThanOrEqualTo(0);
                    validator.RuleFor(importProductPrice => importProductPrice.Price.Sale).GreaterThanOrEqualTo(0).When(importProductPrice => importProductPrice.Price.Sale != null);
                });
            });
        }
    }
}
