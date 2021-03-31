using System.Threading.Tasks;
using Moq;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Model.Search;
using VirtoCommerce.PricingModule.Core.Services;
using VirtoCommerce.SimpleExportImportModule.Core;
using VirtoCommerce.SimpleExportImportModule.Core.Models;
using VirtoCommerce.SimpleExportImportModule.Data.Validation;
using Xunit;

namespace VirtoCommerce.SimpleExportImportModule.Tests
{
    [Trait("Category", "CI")]
    public class ImportProductPriceValidationTests
    {
        private static readonly ImportProductPrice[] Prices = new[]
        {
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
                Price = new Price { MinQuantity = 1, List = 10m, Sale = 9m }
            }
        };

        [Fact]
        public async Task FetchAsync_AfterGetTotalCount_WillStartReadingFromTheStart()
        {
            // Arrange
            var validator = GetValidator();

            // Act
            var validationResult = await validator.ValidateAsync(Prices);

            // Assert
            Assert.Single(validationResult.Errors, validationError => validationError.ErrorCode == ModuleConstants.ValidationErrors.AlreadyExistsError);
        }

        private static IPricingSearchService GetPricingSearchService()
        {
            var pricingSearchServiceMock = new Mock<IPricingSearchService>();
            pricingSearchServiceMock.Setup(service => service.SearchPricesAsync(It.IsAny<PricesSearchCriteria>()))
                .Returns(() => Task.FromResult(new PriceSearchResult
                {
                    Results = new[]
                    {
                        new Price { ProductId = "TestId2", MinQuantity = 1 },
                    },
                    TotalCount = 2
                }));
            return pricingSearchServiceMock.Object;
        }

        private ImportProductPricesValidator GetValidator()
        {
            return new ImportProductPricesValidator(GetPricingSearchService());
        }
    }
}
