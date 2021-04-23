using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using Xunit;

namespace VirtoCommerce.SimpleExportImportModule.Tests
{
    [Trait("Category", "CI")]
    public class CsvPagedPriceDataSourceTests
    {
        private const string CsvFileName = "file.csv";
        private const string CsvHeader = "SKU;Min quantity;List price;Sale price";
        private static readonly string[] CsvRecords = { "TestSku1;1;100;99", "TestSku2;1;10;9.99", ";10;9;" };

        [Theory]
        [MemberData(nameof(GetCsvWithAndWithoutHeader))]
        public void GetTotalCount_Calculate_AndReturnTotalCount(string[] records, string header)
        {
            // Arrange
            var csv = TestHelper.GetCsv(records, header);
            var blobStorageProvider = TestHelper.GetBlobStorageProvider(csv);
            var csvPagedPriceDataSourceFactory = TestHelper.GetCsvPagedPriceDataSourceFactory(blobStorageProvider);
            using var csvPagedPriceDataSource = csvPagedPriceDataSourceFactory.Create(CsvFileName, 10);

            // Act
            var totalCount = csvPagedPriceDataSource.GetTotalCount();

            // Assert
            Assert.Equal(3, totalCount);
        }

        [Fact]
        public void GetTotalCount_CacheTotalCount_AndReturnSameValue()
        {
            // Arrange
            var csv = TestHelper.GetCsv(CsvRecords, CsvHeader);
            var blobStorageProvider = TestHelper.GetBlobStorageProvider(csv);
            var csvPagedPriceDataSourceFactory = TestHelper.GetCsvPagedPriceDataSourceFactory(blobStorageProvider);
            using var csvPagedPriceDataSource = csvPagedPriceDataSourceFactory.Create(CsvFileName, 10);

            // Act
            csvPagedPriceDataSource.GetTotalCount();
            var totalCount = csvPagedPriceDataSource.GetTotalCount();

            // Assert
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
            static async Task FetchAsync()
            {
                // Arrange
                var csv = TestHelper.GetCsv(CsvRecords);
                var blobStorageProvider = TestHelper.GetBlobStorageProvider(csv);
                var csvPagedPriceDataSourceFactory = TestHelper.GetCsvPagedPriceDataSourceFactory(blobStorageProvider);
                using var csvPagedPriceDataSource = csvPagedPriceDataSourceFactory.Create(CsvFileName, 10);

                // Act
                await csvPagedPriceDataSource.FetchAsync();
            }

            // Assert
            await Assert.ThrowsAsync<HeaderValidationException>(FetchAsync);
        }

        [Theory]
        [MemberData(nameof(CsvWithAndWithoutAdditionalData))]
        public async Task FetchAsync_RecordsWithAndWithoutAdditionalColumn_ReturnParsedProductPrices(string[] records)
        {
            // Arrange
            var csv = TestHelper.GetCsv(records, CsvHeader);
            var blobStorageProvider = TestHelper.GetBlobStorageProvider(csv);
            var csvPagedPriceDataSourceFactory = TestHelper.GetCsvPagedPriceDataSourceFactory(blobStorageProvider);
            using var csvPagedPriceDataSource = csvPagedPriceDataSourceFactory.Create(CsvFileName, 10);

            // Act
            await csvPagedPriceDataSource.FetchAsync();

            // Assert
            Assert.Collection(csvPagedPriceDataSource.Items,
                productPrice =>
                {
                    const string productId = "TestId1";
                    Assert.Equal(productId, productPrice.ProductId);
                    Assert.Equal(productId, productPrice.Product?.Id);

                    Assert.Equal(productId, productPrice.Price.ProductId);
                    Assert.Equal(1, productPrice.Price.MinQuantity);
                    Assert.Equal(100, productPrice.Price.List);
                    Assert.Equal(99, productPrice.Price.Sale);
                },
                productPrice =>
                {
                    const string productId = "TestId2";
                    Assert.Equal(productId, productPrice.ProductId);
                    Assert.Equal(productId, productPrice.Product?.Id);
                    Assert.Equal(productId, productPrice.Price.ProductId);
                    Assert.Equal(1, productPrice.Price.MinQuantity);
                    Assert.Equal(10, productPrice.Price.List);
                    Assert.Equal(9.99m, productPrice.Price.Sale);
                },
                productPrice =>
                {
                    const string productId = null;
                    Assert.Equal(productId, productPrice.ProductId);
                    Assert.Equal(productId, productPrice.Product?.Id);
                    Assert.Equal(productId, productPrice.Price.ProductId);
                    Assert.Equal(10, productPrice.Price.MinQuantity);
                    Assert.Equal(9, productPrice.Price.List);
                    Assert.Null(productPrice.Price.Sale);
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
            // Arrange
            var csv = TestHelper.GetCsv(CsvRecords, CsvHeader);
            var blobStorageProvider = TestHelper.GetBlobStorageProvider(csv);
            var csvPagedPriceDataSourceFactory = TestHelper.GetCsvPagedPriceDataSourceFactory(blobStorageProvider);
            using var csvPagedPriceDataSource = csvPagedPriceDataSourceFactory.Create(CsvFileName, 1);

            // Act
            await csvPagedPriceDataSource.FetchAsync();

            // Assert
            Assert.Single(csvPagedPriceDataSource.Items);
        }

        [Fact]
        public async Task FetchAsync_WithSpecifiedPageSize_LoadsOnlyRequestedNumberOfItems()
        {
            // Arrange
            var csv = TestHelper.GetCsv(CsvRecords, CsvHeader);
            await using var stream = TestHelper.GetStream(csv);
            var blobStorageProvider = TestHelper.GetBlobStorageProvider(csv);
            var csvPagedPriceDataSourceFactory = TestHelper.GetCsvPagedPriceDataSourceFactory(blobStorageProvider);
            using var csvPagedPriceDataSource = csvPagedPriceDataSourceFactory.Create(CsvFileName, 1);
            await using var manualCsvStream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
            using var manualCsvReader = new StreamReader(manualCsvStream);
            await manualCsvReader.ReadLineAsync();
            var exptectedRaw = await manualCsvReader.ReadLineAsync();

            // Act
            await csvPagedPriceDataSource.FetchAsync();

            var fetchedCount = csvPagedPriceDataSource.Items.Length;

            // Assert
            Assert.Equal(1, fetchedCount);
            Assert.Equal(exptectedRaw, csvPagedPriceDataSource.Items.First().RawRecord.TrimEnd());
        }

        [Fact]
        public async Task FetchAsync_MultipleTimes_WillUpdateCurrentPageNumber()
        {
            // Arrange
            var csv = TestHelper.GetCsv(CsvRecords, CsvHeader);
            var blobStorageProvider = TestHelper.GetBlobStorageProvider(csv);
            var csvPagedPriceDataSourceFactory = TestHelper.GetCsvPagedPriceDataSourceFactory(blobStorageProvider);
            using var csvPagedPriceDataSource = csvPagedPriceDataSourceFactory.Create(CsvFileName, 1);

            // Act
            await csvPagedPriceDataSource.FetchAsync();
            await csvPagedPriceDataSource.FetchAsync();

            // Assert
            Assert.Equal(2, csvPagedPriceDataSource.CurrentPageNumber);
        }

        [Theory]
        [MemberData(nameof(CsvWithInvalidRows))]
        public async Task FetchAsync_WithInvalidRows_IgnoreInvalidRows(string[] records)
        {
            // Arrange
            var csv = TestHelper.GetCsv(records, CsvHeader);
            var blobStorageProvider = TestHelper.GetBlobStorageProvider(csv);
            var csvPagedPriceDataSourceFactory = TestHelper.GetCsvPagedPriceDataSourceFactory(blobStorageProvider);
            using var csvPagedPriceDataSource = csvPagedPriceDataSourceFactory.Create(CsvFileName, 10);

            // Act
            await csvPagedPriceDataSource.FetchAsync();

            // Assert
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

        [Fact]
        public async Task FetchAsync_WithSpecifiedPageSize_WillReturnSpecifiedNumberOfItems()
        {
            // Arrange
            var csv = TestHelper.GetCsv(CsvRecords, CsvHeader);
            var blobStorageProvider = TestHelper.GetBlobStorageProvider(csv);
            var csvPagedPriceDataSourceFactory = TestHelper.GetCsvPagedPriceDataSourceFactory(blobStorageProvider);
            using var csvPagedPriceDataSource = csvPagedPriceDataSourceFactory.Create(CsvFileName, 1);

            // Act
            await csvPagedPriceDataSource.FetchAsync();
            await csvPagedPriceDataSource.FetchAsync();

            // Assert
            Assert.Equal("TestSku2", csvPagedPriceDataSource.Items.Single().Sku);
        }

        [Fact]
        public async Task FetchAsync_AfterGetTotalCount_WillStartReadingFromTheSamePosition()
        {
            // Arrange
            var csv = TestHelper.GetCsv(CsvRecords, CsvHeader);
            var blobStorageProvider = TestHelper.GetBlobStorageProvider(csv);
            var csvPagedPriceDataSourceFactory = TestHelper.GetCsvPagedPriceDataSourceFactory(blobStorageProvider);
            using var csvPagedPriceDataSource = csvPagedPriceDataSourceFactory.Create(CsvFileName, 1);

            // Act
            await csvPagedPriceDataSource.FetchAsync();
            csvPagedPriceDataSource.GetTotalCount();
            await csvPagedPriceDataSource.FetchAsync();

            // Assert
            Assert.Equal("TestSku2", csvPagedPriceDataSource.Items.Single().Sku);
        }

        [Fact]
        public async Task FetchAsync_BeforeEndOfCsvFile_WillReturnTrue()
        {
            // Arrange
            var csv = TestHelper.GetCsv(CsvRecords, CsvHeader);
            var blobStorageProvider = TestHelper.GetBlobStorageProvider(csv);
            var csvPagedPriceDataSourceFactory = TestHelper.GetCsvPagedPriceDataSourceFactory(blobStorageProvider);
            using var csvPagedPriceDataSource = csvPagedPriceDataSourceFactory.Create(CsvFileName, 1);

            // Act
            var result = await csvPagedPriceDataSource.FetchAsync();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task FetchAsync_AfterEndOfCsvFile_WillReturnFalse()
        {
            // Arrange
            var csv = TestHelper.GetCsv(CsvRecords, CsvHeader);
            var blobStorageProvider = TestHelper.GetBlobStorageProvider(csv);
            var csvPagedPriceDataSourceFactory = TestHelper.GetCsvPagedPriceDataSourceFactory(blobStorageProvider);
            using var csvPagedPriceDataSource = csvPagedPriceDataSourceFactory.Create(CsvFileName, 10);

            // Act
            await csvPagedPriceDataSource.FetchAsync();
            var result = await csvPagedPriceDataSource.FetchAsync();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task FetchAsync_AfterEndOfCsvFile_WillFetchNoItems()
        {
            // Arrange
            var csv = TestHelper.GetCsv(CsvRecords, CsvHeader);
            var blobStorageProvider = TestHelper.GetBlobStorageProvider(csv);
            var csvPagedPriceDataSourceFactory = TestHelper.GetCsvPagedPriceDataSourceFactory(blobStorageProvider);
            using var csvPagedPriceDataSource = csvPagedPriceDataSourceFactory.Create(CsvFileName, 10);

            // Act
            await csvPagedPriceDataSource.FetchAsync();
            await csvPagedPriceDataSource.FetchAsync();

            // Assert
            Assert.Empty(csvPagedPriceDataSource.Items);
        }
    }
}
