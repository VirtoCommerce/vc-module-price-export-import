using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using Moq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.SimpleExportImportModule.Data.Services;
using Xunit;

namespace VirtoCommerce.SimpleExportImportModule.Tests
{
    [Trait("Category", "CI")]
    public class CsvPagedPriceDataSourceTests
    {
        private const string CsvHeader = "SKU;Min quantity;List price;Sale price";
        private static readonly string[] CsvRecords = { "TestSku1;1;100;99", "TestSku2;1;10;9.99", ";10;9;" };

        [Theory]
        [MemberData(nameof(GetCsvWithAndWithoutHeader))]
        public async Task GetTotalCount_Calculate_AndReturnTotalCount(string[] records, string header)
        {
            await using var stream = await GetStream(GetCsv(records, header));
            var csvPagedPriceDataSourceFactory = GetCsvPagedPriceDataSourceFactory();
            using var csvPagedPriceDataSource = csvPagedPriceDataSourceFactory.Create(stream, 10);

            var totalCount = csvPagedPriceDataSource.GetTotalCount();

            Assert.Equal(3, totalCount);
        }

        [Fact]
        public async Task GetTotalCount_CacheTotalCount_AndReturnSameValue()
        {
            await using var stream = await GetStream(GetCsv(CsvRecords, CsvHeader));
            var csvPagedPriceDataSourceFactory = GetCsvPagedPriceDataSourceFactory();
            using var csvPagedPriceDataSource = csvPagedPriceDataSourceFactory.Create(stream, 10);

            csvPagedPriceDataSource.GetTotalCount();
            await stream.DisposeAsync();
            var totalCount = csvPagedPriceDataSource.GetTotalCount();

            Assert.Equal(3, totalCount);
        }

        public static IEnumerable<object[]> GetCsvWithAndWithoutHeader()
        {
            yield return new object[] { CsvRecords, CsvHeader };
            yield return new object[] { CsvRecords, null };
        }

        [Fact]
        public async Task FetchAsync_WithMissedHeader_ThrowsException()
        {
            await using var stream = await GetStream(GetCsv(CsvRecords));
            var csvPagedPriceDataSourceFactory = GetCsvPagedPriceDataSourceFactory();
            using var csvPagedPriceDataSource = csvPagedPriceDataSourceFactory.Create(stream, 10);

            await Assert.ThrowsAsync<HeaderValidationException>(() => csvPagedPriceDataSource.FetchAsync());
        }

        [Theory]
        [MemberData(nameof(CsvWithAndWithoutAdditionalData))]
        public async Task FetchAsync_RecordsWithAndWithoutAdditionalColumn_ReturnParsedProductPrices(string[] records)
        {
            await using var stream = await GetStream(GetCsv(records, CsvHeader));
            var csvPagedPriceDataSourceFactory = GetCsvPagedPriceDataSourceFactory();
            using var csvPagedPriceDataSource = csvPagedPriceDataSourceFactory.Create(stream, 10);

            await csvPagedPriceDataSource.FetchAsync();

            Assert.Collection(csvPagedPriceDataSource.Items,
                productPrice =>
                {
                    const string productId = "TestId1";
                    Assert.Equal(productId, productPrice.ProductId);
                    Assert.Equal(productId, productPrice.Product?.Id);
                    Assert.Single(productPrice.Prices);
                    Assert.Single(productPrice.Prices,
                        x => x.ProductId == productId && x.MinQuantity == 1 && x.List == 100 && x.Sale == 99);
                },
                productPrice =>
                {
                    const string productId = "TestId2";
                    Assert.Equal(productId, productPrice.ProductId);
                    Assert.Equal(productId, productPrice.Product?.Id);
                    Assert.Single(productPrice.Prices);
                    Assert.Single(productPrice.Prices,
                        x => x.ProductId == productId && x.MinQuantity == 1 && x.List == 10 && x.Sale == 9.99m);
                },
                productPrice =>
                {
                    const string productId = null;
                    Assert.Equal(productId, productPrice.ProductId);
                    Assert.Equal(productId, productPrice.Product?.Id);
                    Assert.Single(productPrice.Prices);
                    Assert.Single(productPrice.Prices,
                        x => x.ProductId == productId && x.MinQuantity == 10 && x.List == 9 && x.Sale == null);
                });
        }

        public static IEnumerable<object[]> CsvWithAndWithoutAdditionalData
        {
            get
            {
                yield return new object[] { CsvRecords };
                yield return new object[]
                {
                    CsvRecords.Select(x => $"{x};AdditionalValue").ToArray()
                };
            }
        }

        [Fact]
        public async Task FetchAsync_WithSpecifiedPageSize_ReturnsOnlyRequestedNumberOfItems()
        {
            await using var stream = await GetStream(GetCsv(CsvRecords, CsvHeader));
            var csvPagedPriceDataSourceFactory = GetCsvPagedPriceDataSourceFactory();
            using var csvPagedPriceDataSource = csvPagedPriceDataSourceFactory.Create(stream, 1);

            await csvPagedPriceDataSource.FetchAsync();

            Assert.Single(csvPagedPriceDataSource.Items);
        }

        [Fact]
        public async Task FetchAsync_WithSpecifiedPageSize_LoadsOnlyRequestedNumberOfItems()
        {
            var csv = GetCsv(CsvRecords, CsvHeader);
            await using var stream = await GetStream(csv);
            var csvPagedPriceDataSourceFactory = GetCsvPagedPriceDataSourceFactory();
            using var csvPagedPriceDataSource = csvPagedPriceDataSourceFactory.Create(stream, 1);
            await using var manualCsvStream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
            using var manualCsvReader = new StreamReader(manualCsvStream);
            await manualCsvReader.ReadLineAsync();
            await manualCsvReader.ReadLineAsync();

            await csvPagedPriceDataSource.FetchAsync();

            Assert.Equal(manualCsvStream.Position, stream.Position);
        }

        [Theory]
        [MemberData(nameof(CsvWithInvalidRows))]
        public async Task FetchAsync_WithInvalidRows_IgnoreInvalidRows(string[] records)
        {
            await using var stream = await GetStream(GetCsv(records, CsvHeader));
            var csvPagedPriceDataSourceFactory = GetCsvPagedPriceDataSourceFactory();
            using var csvPagedPriceDataSource = csvPagedPriceDataSourceFactory.Create(stream, 10);

            await csvPagedPriceDataSource.FetchAsync();

            Assert.Equal(3, csvPagedPriceDataSource.Items.Length);
        }

        public static IEnumerable<object[]> CsvWithInvalidRows
        {
            get
            {
                // Invalid row
                var csvRecords = new List<string>(CsvRecords);
                csvRecords.Insert(2, "x\r\n");
                yield return new object[] { csvRecords };

                // Wrong delimiter
                csvRecords = new List<string>(CsvRecords);
                csvRecords.Insert(2, "TestSku2,1,10,9.99");
                yield return new object[] { csvRecords };

                // Missed column
                csvRecords = new List<string>(CsvRecords);
                csvRecords.Insert(2, "TestSku2;1");
                yield return new object[] { csvRecords };

                // Missed required value
                csvRecords = new List<string>(CsvRecords);
                csvRecords.Insert(2, "TestSku2;;10;9.99");
                yield return new object[] { csvRecords };
                csvRecords = new List<string>(CsvRecords);
                csvRecords.Insert(2, "TestSku2;1;;9.99");
                yield return new object[] { csvRecords };

                // Invalid value
                csvRecords = new List<string>(CsvRecords);
                csvRecords.Insert(2, "TestSku2;string;10;9.99");
                yield return new object[] { csvRecords };

                // Out of range
                csvRecords = new List<string>(CsvRecords);
                csvRecords.Insert(2, $"TestSku2;1;1{decimal.MaxValue};9.99");
                yield return new object[] { csvRecords };
            }
        }

        private static IProductSearchService GetProductSearchService()
        {
            var productSearchServiceMock = new Mock<IProductSearchService>();
            productSearchServiceMock.Setup(service => service.SearchProductsAsync(It.IsAny<ProductSearchCriteria>()))
                .Returns(() => Task.FromResult(new ProductSearchResult
                {
                    Results = new[]
                    {
                        new CatalogProduct { Id = "TestId1", Code = "TestSku1" },
                        new CatalogProduct { Id = "TestId2", Code = "TestSku2" },
                    },
                    TotalCount = 2
                }));
            return productSearchServiceMock.Object;
        }

        private static CsvPagedPriceDataSourceFactory GetCsvPagedPriceDataSourceFactory()
        {
            return new CsvPagedPriceDataSourceFactory(GetProductSearchService());
        }

        private static async Task<Stream> GetStream(string csv)
        {
            var stream = new MemoryStream();
            await using var writer = new StreamWriter(stream, leaveOpen: true);
            await writer.WriteAsync(csv);
            await writer.FlushAsync();
            stream.Position = 0;
            return stream;
        }

        private static string GetCsv(IEnumerable<string> records, string header = null)
        {
            var csv = new StringBuilder();

            if (header != null)
            {
                csv.AppendLine(header);
            }

            foreach (var record in records)
            {
                csv.AppendLine(record);
            }

            return csv.ToString();
        }
    }
}
