using FluentValidation;
using VirtoCommerce.PriceExportImportModule.Core.Models;
using VirtoCommerce.PricingModule.Core.Services;

namespace VirtoCommerce.PriceExportImportModule.Data.Validation
{
    public sealed class ImportProductPricesValidator : AbstractValidator<ImportProductPrice[]>
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
                RuleFor(importProductPrices => importProductPrices)
                    .SetValidator(_ => new ImportProductPricesExistenceValidator(_pricingSearchService, ImportProductPricesExistenceValidationMode.NotExists), "default");
            });
            RuleSet(nameof(ImportMode.UpdateOnly), () =>
            {
                RuleFor(importProductPrices => importProductPrices).SetValidator(_ => new ImportProductPricesAreNotDuplicatesValidator(ImportMode.UpdateOnly), "default");
                RuleFor(importProductPrices => importProductPrices)
                    .SetValidator(_ => new ImportProductPricesExistenceValidator(_pricingSearchService, ImportProductPricesExistenceValidationMode.Exists), "default");
            });
            RuleSet(nameof(ImportMode.CreateAndUpdate),
                () => { RuleFor(importProductPrices => importProductPrices).SetValidator(_ => new ImportProductPricesAreNotDuplicatesValidator(ImportMode.CreateAndUpdate), "default"); });
            RuleSet($"{nameof(ImportMode.CreateOnly)},{nameof(ImportMode.UpdateOnly)},{nameof(ImportMode.CreateAndUpdate)}", () =>
            {
                RuleForEach(importProductPrices => importProductPrices).SetValidator(importProductPrice => new ImportProductPriceSkuNotEmptyValidator(), "default");
                RuleForEach(importProductPrices => importProductPrices).SetValidator(importProductPrice => new ImportProductPriceProductExistsValidator(), "default");
                RuleForEach(importProductPrice => importProductPrice).SetValidator(_ => new ImportProductPriceNumbersValidator(), "default");
            });
        }
    }
}
