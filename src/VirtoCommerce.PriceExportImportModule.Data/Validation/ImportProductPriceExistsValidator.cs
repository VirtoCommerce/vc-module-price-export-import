using System.Linq;
using FluentValidation;
using VirtoCommerce.PriceExportImportModule.Core;
using VirtoCommerce.PriceExportImportModule.Core.Models;
using VirtoCommerce.PricingModule.Core.Model;

namespace VirtoCommerce.PriceExportImportModule.Data.Validation
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
                    var existingPrices = (Price[])context.RootContextData[ImportProductPricesExistenceValidator.ExistingPrices];
                    var exist = existingPrices.Any(existingPrice =>
                        existingPrice.ProductId == price.ProductId &&
                        existingPrice.MinQuantity == price.Price.MinQuantity);
                    return exist;
                })
                .WithErrorCode(ModuleConstants.ValidationErrors.NotExistsError)
                .WithState(importProductPrice => new ImportValidationState { InvalidImportProductPrice = importProductPrice })
                .WithMessage("This price does not exist.");
        }
    }
}
