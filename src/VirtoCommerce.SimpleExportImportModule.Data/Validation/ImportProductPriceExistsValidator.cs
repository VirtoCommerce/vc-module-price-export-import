using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.SimpleExportImportModule.Core;
using VirtoCommerce.SimpleExportImportModule.Core.Models;

namespace VirtoCommerce.SimpleExportImportModule.Data.Validation
{
    public sealed class ImportProductPriceExistsValidator: AbstractValidator<ImportProductPrice>
    {
        private readonly IEnumerable<Price> _existingPrices;

        public ImportProductPriceExistsValidator(IEnumerable<Price> existingPrices)
        {
            _existingPrices = existingPrices;
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(price => price)
                .Must(price => !_existingPrices.Any(existingPrice =>
                    existingPrice.ProductId == price.ProductId &&
                    existingPrice.MinQuantity == price.Price.MinQuantity))
                .WithErrorCode(ModuleConstants.ValidationErrors.AlreadyExistsError);
        }
    }
}
