using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Moq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.PriceExportImportModule.Core;
using VirtoCommerce.PriceExportImportModule.Core.Models;
using VirtoCommerce.PriceExportImportModule.Data.Validation;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Model.Search;
using VirtoCommerce.PricingModule.Core.Services;
using Xunit;

namespace VirtoCommerce.PriceExportImportModule.Tests
{
    [Trait("Category", "CI")]
    public class ImportProductPriceValidationTests
    {
        private static readonly ImportProductPrice[] Prices = {
            new ImportProductPrice
            {
                ProductId = "TestId1",
                Sku = "TestSku1",
                Price = new Price { MinQuantity = 1, List = 10m, Sale = 9m }
            },
            new ImportProductPrice
            {
                ProductId = "TestId2",
                Sku = "TestSku2",
                Price = new Price { MinQuantity = 1, List = 10m, Sale = 9m }
            },
            new ImportProductPrice
            {
                ProductId = "TestId3",
                Sku = "TestSku3",
                Price = new Price { MinQuantity = -1, List = -10m, Sale = -9m }
            },
            new ImportProductPrice
            {
                ProductId = "TestId1",
                Product = new CatalogProduct { Id = "TestId1" },
                Sku = "TestSku1",
                Price = new Price { MinQuantity = 1, List = 20m, Sale = 19m }
            },
            new ImportProductPrice
            {
                //ProductId = null,
                //Product = new CatalogProduct { Id = "TestId1" },
                Sku = "",
                Price = new Price { MinQuantity = 1, List = 20m, Sale = 19m }
            }
        };

        [Fact]
        public async Task ValidateAsync_DuplicatesOnCreate_WillFailAndReportLast()
        {
            // Arrange
            var validator = GetValidator();

            // Act
            var validationResult = await validator.ValidateAsync(Prices, options => options.IncludeRuleSets(ImportMode.CreateOnly.ToString()));

            // Assert
            var error = validationResult.Errors.FirstOrDefault(validationError => validationError.ErrorCode == ModuleConstants.ValidationErrors.DuplicateError);
            Assert.NotNull(error);
            Assert.Equal(20, (error.CustomState as ImportValidationState)?.InvalidImportProductPrice.Price.List);
        }

        [Fact]
        public async Task ValidateAsync_DuplicatesOnUpdate_WillFailAndReportFirst()
        {
            // Arrange
            var validator = GetValidator();

            // Act
            var validationResult = await validator.ValidateAsync(Prices, options => options.IncludeRuleSets(ImportMode.UpdateOnly.ToString()));

            // Assert
            var error = validationResult.Errors.FirstOrDefault(validationError => validationError.ErrorCode == ModuleConstants.ValidationErrors.DuplicateError);
            Assert.NotNull(error);
            Assert.Equal(10, (error.CustomState as ImportValidationState)?.InvalidImportProductPrice.Price.List);
        }

        [Fact]
        public async Task ValidateAsync_DuplicatesOnCreateAndUpdate_WillFailAndReportFirst()
        {
            // Arrange
            var validator = GetValidator();

            // Act
            var validationResult = await validator.ValidateAsync(Prices, options => options.IncludeRuleSets(ImportMode.CreateAndUpdate.ToString()));

            // Assert
            var error = validationResult.Errors.FirstOrDefault(validationError => validationError.ErrorCode == ModuleConstants.ValidationErrors.DuplicateError);
            Assert.NotNull(error);
            Assert.Equal(10, (error.CustomState as ImportValidationState)?.InvalidImportProductPrice.Price.List);
        }

        [Fact]
        public async Task ValidateAsync_AlreadyExistsOnCreate_WillFail()
        {
            // Arrange
            var validator = GetValidator();

            // Act
            var validationResult = await validator.ValidateAsync(Prices, options => options.IncludeRuleSets(ImportMode.CreateOnly.ToString()));

            // Assert
            Assert.Single(validationResult.Errors, validationError => validationError.ErrorCode == ModuleConstants.ValidationErrors.AlreadyExistsError);
        }

        [Fact]
        public async Task ValidateAsync_NotExistsOnCreate_WillFail()
        {
            // Arrange
            var validator = GetValidator();

            // Act
            var validationResult = await validator.ValidateAsync(Prices, options => options.IncludeRuleSets(ImportMode.UpdateOnly.ToString()));

            // Assert
            var errors = validationResult.Errors.Where(validationError => validationError.ErrorCode == ModuleConstants.ValidationErrors.NotExistsError).ToArray();
            Assert.NotNull(errors);
            Assert.Equal(4, errors.Length);
        }

        [Theory]
        [InlineData(ImportMode.CreateOnly)]
        [InlineData(ImportMode.UpdateOnly)]
        [InlineData(ImportMode.CreateAndUpdate)]
        public async Task ValidateAsync_ProductMissed_WillFail(ImportMode importMode)
        {
            // Arrange
            var validator = GetValidator();

            // Act
            var validationResult = await validator.ValidateAsync(Prices, options => options.IncludeRuleSets(importMode.ToString()));

            // Assert
            var errors = validationResult.Errors.Where(validationError => validationError.ErrorCode == ModuleConstants.ValidationErrors.ProductMissingError).ToArray();
            Assert.NotNull(errors);
            Assert.Equal(4, errors.Length);
        }

        [Fact]
        public async Task ValidateAsync_NegativeNumbers_WillFail()
        {
            // Arrange
            var validator = GetValidator();

            // Act
            var validationResult = await validator.ValidateAsync(Prices, options => options.IncludeRuleSets(ImportMode.CreateOnly.ToString()));

            // Assert
            Assert.Equal(3, validationResult.Errors.Count(validationError => validationError.ErrorCode == ModuleConstants.ValidationErrors.NegativeNumbers));
        }

        [Fact]
        public async Task ValidateAsync_SkuNotEmpty_WillFail()
        {
            // Arrange
            var validator = GetValidator();

            // Act
            var validationResult = await validator.ValidateAsync(Prices, options => options.IncludeRuleSets(ImportMode.CreateOnly.ToString()));

            // Assert
            Assert.Equal(1, validationResult.Errors.Count(validationError => validationError.ErrorCode == ModuleConstants.ValidationErrors.SkuIsEmpty));
        }

        private static IPriceSearchService GetPricingSearchService()
        {
            var pricingSearchServiceMock = new Mock<IPriceSearchService>();
            pricingSearchServiceMock.Setup(service => service.SearchAsync(It.IsAny<PricesSearchCriteria>(), It.IsAny<bool>()))
                .Returns(() => Task.FromResult(new PriceSearchResult
                {
                    Results = new[]
                    {
                        new Price { ProductId = "TestId2", MinQuantity = 1 },
                    },
                    TotalCount = 1
                }));
            return pricingSearchServiceMock.Object;
        }

        private ImportProductPricesValidator GetValidator()
        {
            return new ImportProductPricesValidator(GetPricingSearchService());
        }
    }
}
