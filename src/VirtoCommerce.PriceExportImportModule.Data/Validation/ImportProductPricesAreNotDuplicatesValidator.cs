using System.Linq;
using FluentValidation;
using FluentValidation.Validators;
using VirtoCommerce.PriceExportImportModule.Core.Models;

namespace VirtoCommerce.PriceExportImportModule.Data.Validation
{
    public sealed class ImportProductPricesAreNotDuplicatesValidator : AbstractValidator<ImportProductPrice[]>
    {
        internal const string Duplicates = nameof(Duplicates);

        private readonly ImportMode _importMode;

        public ImportProductPricesAreNotDuplicatesValidator(ImportMode importMode)
        {
            _importMode = importMode;
            AttachValidators();
        }

        private void AttachValidators()
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
            context.ParentContext.RootContextData[Duplicates] = duplicates;
        }
    }
}
