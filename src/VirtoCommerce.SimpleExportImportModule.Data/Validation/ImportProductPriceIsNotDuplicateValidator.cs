using System.Linq;
using FluentValidation;
using VirtoCommerce.SimpleExportImportModule.Core;
using VirtoCommerce.SimpleExportImportModule.Core.Models;

namespace VirtoCommerce.SimpleExportImportModule.Data.Validation
{
    public sealed class ImportProductPriceIsNotDuplicateValidator: AbstractValidator<ImportProductPrice>
    {
        public ImportProductPriceIsNotDuplicateValidator()
        {
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(price => price)
                .Must((_, price, context) =>
                {
                    var duplicates = (ImportProductPrice[])context.ParentContext.RootContextData[ImportProductPricesAreNotDuplicatesValidator.Duplicates];
                    return !duplicates.Contains(price);
                })
                .WithErrorCode(ModuleConstants.ValidationErrors.DuplicateError)
                .WithState(importProductPrice => new ImportValidationState
                {
                    InvalidImportProductPrice = importProductPrice
                });
        }
    }
}
