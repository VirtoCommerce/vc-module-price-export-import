using FluentValidation;
using VirtoCommerce.SimpleExportImportModule.Core;
using VirtoCommerce.SimpleExportImportModule.Core.Models;

namespace VirtoCommerce.SimpleExportImportModule.Data.Validation
{
    public class ImportProductPriceNumbersValidator : AbstractValidator<ImportProductPrice>
    {
        public ImportProductPriceNumbersValidator()
        {
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(importProductPrice => importProductPrice.Price.MinQuantity).GreaterThan(0)
                .WithErrorCode(ModuleConstants.ValidationErrors.NegativeNumbers);
            RuleFor(importProductPrice => importProductPrice.Price.List).GreaterThanOrEqualTo(0)
                .WithErrorCode(ModuleConstants.ValidationErrors.NegativeNumbers);
            RuleFor(importProductPrice => importProductPrice.Price.Sale).GreaterThanOrEqualTo(0).When(importProductPrice => importProductPrice.Price.Sale != null)
                .WithErrorCode(ModuleConstants.ValidationErrors.NegativeNumbers);
        }
    }
}
