using FluentValidation;
using VirtoCommerce.SimpleExportImportModule.Core;
using VirtoCommerce.SimpleExportImportModule.Core.Models;

namespace VirtoCommerce.SimpleExportImportModule.Data.Validation
{
    public sealed class ImportProductPriceSkuNotEmptyValidator : AbstractValidator<ImportProductPrice>
    {
        public ImportProductPriceSkuNotEmptyValidator()
        {
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(importProductPrice => importProductPrice.Sku).NotEmpty()
                .WithErrorCode(ModuleConstants.ValidationErrors.SkuIsEmpty)
                .WithState(importProductPrice => new ImportValidationState { InvalidImportProductPrice = importProductPrice, FieldName = "SKU" })
                .WithMessage("SKU should not be empty.");
        }
    }
}
