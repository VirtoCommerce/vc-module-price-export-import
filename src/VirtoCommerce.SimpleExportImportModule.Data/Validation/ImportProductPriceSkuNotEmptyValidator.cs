using FluentValidation;
using VirtoCommerce.SimpleExportImportModule.Core;
using VirtoCommerce.SimpleExportImportModule.Core.Models;

namespace VirtoCommerce.SimpleExportImportModule.Data.Validation
{
    public class ImportProductPriceSkuNotEmptyValidator : AbstractValidator<ImportProductPrice>
    {
        public ImportProductPriceSkuNotEmptyValidator()
        {
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(importProductPrice => importProductPrice.Sku).NotEmpty()
                .WithErrorCode(ModuleConstants.ValidationErrors.SkuIsEmpty);
        }
    }
}
