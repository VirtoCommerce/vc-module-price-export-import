using FluentValidation;
using VirtoCommerce.SimpleExportImportModule.Core;
using VirtoCommerce.SimpleExportImportModule.Core.Models;

namespace VirtoCommerce.SimpleExportImportModule.Data.Validation
{
    public class ImportProductPriceProductExistsValidator: AbstractValidator<ImportProductPrice>
    {

        public ImportProductPriceProductExistsValidator()
        {
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(importProductPrice => importProductPrice.ProductId).NotEmpty().DependentRules(() =>
            {
                RuleFor(importProductPrice => importProductPrice.Product).NotNull().WithErrorCode(ModuleConstants.ValidationErrors.ProductMissingError);
            }).WithErrorCode(ModuleConstants.ValidationErrors.ProductMissingError);
        }
    }
}
