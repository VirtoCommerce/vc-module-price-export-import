using FluentValidation;
using VirtoCommerce.PriceExportImportModule.Core;
using VirtoCommerce.PriceExportImportModule.Core.Models;

namespace VirtoCommerce.PriceExportImportModule.Data.Validation
{
    public sealed class ImportProductPriceProductExistsValidator : AbstractValidator<ImportProductPrice>
    {

        public ImportProductPriceProductExistsValidator()
        {
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(importProductPrice => importProductPrice.ProductId).NotEmpty().DependentRules(() =>
                {
                    RuleFor(importProductPrice => importProductPrice.Product).NotNull()
                        .WithErrorCode(ModuleConstants.ValidationErrors.ProductMissingError)
                        .WithState(importProductPrice => new ImportValidationState { InvalidImportProductPrice = importProductPrice })
                        .WithMessage("SKU does not exists in the related catalog.");
                })
                .WithErrorCode(ModuleConstants.ValidationErrors.ProductMissingError)
                .WithState(importProductPrice => new ImportValidationState { InvalidImportProductPrice = importProductPrice })
                .WithMessage("SKU does not exists in the related catalog.");
        }
    }
}
