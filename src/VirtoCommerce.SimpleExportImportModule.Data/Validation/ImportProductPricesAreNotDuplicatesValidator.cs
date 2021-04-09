using System.Linq;
using FluentValidation;
using FluentValidation.Validators;
using VirtoCommerce.SimpleExportImportModule.Core.Models;

namespace VirtoCommerce.SimpleExportImportModule.Data.Validation
{
    public class ImportProductPricesAreNotDuplicatesValidator: AbstractValidator<ImportProductPrice[]>
    {
        private readonly ImportMode _importMode;

        public ImportProductPricesAreNotDuplicatesValidator(ImportMode importMode)
        {
            _importMode = importMode;
            AttachValidators();
        }

        public void AttachValidators()
        {
            RuleFor(importProductPrices => importProductPrices)
                .Custom((importProductPrices, context) => GetDuplicates(importProductPrices, context, _importMode))
                .ForEach(rule => rule.SetValidator(_ => new ImportProductPriceIsNotDuplicateValidator()));
        }

        private void GetDuplicates(ImportProductPrice[] importProductPrices, CustomContext context, ImportMode importMode)
        {
            var duplicates = importProductPrices.GroupBy(importProductPrice => new { importProductPrice.Sku, importProductPrice.Price.MinQuantity })
                .SelectMany(group => importMode == ImportMode.CreateOnly ? group.Skip(1) : group.Take(group.Count() - 1))
                .ToArray();
            context.ParentContext.RootContextData[ImportProductPriceIsNotDuplicateValidator.Duplicates] = duplicates;
        }
    }
}
