using System.Linq;
using FluentValidation;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.SimpleExportImportModule.Core;
using VirtoCommerce.SimpleExportImportModule.Core.Models;

namespace VirtoCommerce.SimpleExportImportModule.Data.Validation
{
    public sealed class ImportProductPriceExistsValidator : AbstractValidator<ImportProductPrice>
    {

        public ImportProductPriceExistsValidator()
        {
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(price => price)
                .Must((_, price, context) =>
                {
                    var existingPrices = (Price[]) context.ParentContext.RootContextData[ImportProductPricesExistenceValidator.ExistingPrices];
                    var exist = existingPrices.Any(existingPrice =>
                        existingPrice.ProductId == price.ProductId &&
                        existingPrice.MinQuantity == price.Price.MinQuantity);
                    return exist;
                })
                .WithErrorCode(ModuleConstants.ValidationErrors.NotExistsError)
                .WithState(importProductPrice => new ImportValidationState { InvalidImportProductPrice = importProductPrice });
        }
    }
}
