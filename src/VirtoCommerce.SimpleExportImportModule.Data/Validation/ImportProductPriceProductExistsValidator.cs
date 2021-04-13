using FluentValidation;
using VirtoCommerce.SimpleExportImportModule.Core;
using VirtoCommerce.SimpleExportImportModule.Core.Models;

namespace VirtoCommerce.SimpleExportImportModule.Data.Validation
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
                        .WithMessage("SKU doesn’t exists in the related catalog");
                })
                .WithErrorCode(ModuleConstants.ValidationErrors.ProductMissingError)
                .WithState(importProductPrice => new ImportValidationState { InvalidImportProductPrice = importProductPrice })
                .WithMessage("SKU doesn’t exists in the related catalog");
        }
    }
}
