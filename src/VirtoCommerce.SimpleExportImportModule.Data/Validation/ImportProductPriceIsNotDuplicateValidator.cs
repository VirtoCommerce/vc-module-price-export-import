using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using VirtoCommerce.SimpleExportImportModule.Core;
using VirtoCommerce.SimpleExportImportModule.Core.Models;

namespace VirtoCommerce.SimpleExportImportModule.Data.Validation
{
    public sealed class ImportProductPriceIsNotDuplicateValidator: AbstractValidator<ImportProductPrice>
    {
        private readonly IEnumerable<ImportProductPrice> _duplicates;

        public ImportProductPriceIsNotDuplicateValidator(IEnumerable<ImportProductPrice> duplicates)
        {
            _duplicates = duplicates;
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(price => price)
                .Must(price => !_duplicates.Contains(price))
                .WithErrorCode(ModuleConstants.ValidationErrors.DuplicateError);
        }
    }
}
