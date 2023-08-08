using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PriceExportImportModule.Core.Models;
using VirtoCommerce.PriceExportImportModule.Core.Services;
using VirtoCommerce.PriceExportImportModule.Data.Services;
using VirtoCommerce.PriceExportImportModule.Data.Validation;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Model.Search;
using VirtoCommerce.PricingModule.Core.Services;
using Xunit;

namespace VirtoCommerce.PriceExportImportModule.Tests
{
    [Trait("Category", "CI")]
    public class CsvPagedPriceDataImporterReadingErrorsTests
    {
        private const string CsvHeader = "SKU;Min quantity;List price;Sale price";
        private static readonly string[] validRows = { "TestSku1;2;10.99;9.99", "TestSku2;2;10.99;9" };

        [Theory]
        [InlineData("XXX;Text;10.9;Text", "Min quantity")]
        [InlineData("\"XXX\";2;Text;9.9", "List price")]
        [InlineData("SKU1;1;10.9;Text", "Sale price")]
        public async Task ImportAsync_InvalidColumnValueDuringImport_WillReportError(string row, string invalidFieldName)
        {
            // Arrange
            var request = TestHelper.CreateImportDataRequest();
            var cancellationTokenWrapper = GetCancellationTokenWrapper();
            var progressInfos = new List<ImportProgressInfo>();
            void ProgressCallback(ImportProgressInfo progressInfo)
            {
                progressInfos.Add((ImportProgressInfo)progressInfo.Clone());
            }

            var invalidRows = new[] { row };

            var errorReporterStream = new MemoryStream();

            var importReporterFactoryMock = new Mock<ICsvPriceImportReporterFactory>();
            var importReporterMock = new Mock<ICsvPriceImportReporter>();
            ImportError errorForAssertion = null;

            importReporterMock.Setup(x => x.Write(It.IsAny<ImportError>()))
                .Callback<ImportError>(error => errorForAssertion = error);

            importReporterFactoryMock.Setup(x => x.CreateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(importReporterMock.Object);

            var allRows = validRows.Union(invalidRows).ToArray();

            var importer = GetCsvPagedPriceDataImporter(GetBlobStorageProvider(CsvHeader, allRows, errorReporterStream), importReporterFactoryMock.Object);

            // Act
            await importer.ImportAsync(request, ProgressCallback, cancellationTokenWrapper);

            // Assert
            var errorProgressInfo = progressInfos.LastOrDefault();
            Assert.Equal(validRows.Length + invalidRows.Length, errorProgressInfo?.ProcessedCount);
            Assert.Equal(2, errorProgressInfo?.CreatedCount);
            Assert.Equal(0, errorProgressInfo?.UpdatedCount);
            Assert.Equal(invalidRows.Length, errorProgressInfo?.ErrorCount);
            Assert.NotNull(errorProgressInfo?.Description);
            Assert.StartsWith("Import completed with errors", errorProgressInfo?.Description);

            importReporterMock.Verify(x => x.Write(It.IsAny<ImportError>()), Times.Once);

            Assert.Equal($"This row has invalid value in the column {invalidFieldName}.", errorForAssertion.Error);
            Assert.Equal($"{invalidRows.First()}", errorForAssertion.RawRow.TrimEnd());
        }


        [Theory]
        [InlineData("XXX;\"9;10.9;9")]
        [InlineData("\"XXX\";2;\"9;9.9")]
        public async Task ImportAsync_BadDataFoundDuringImport_WillReportError(string row)
        {
            // Arrange
            var request = TestHelper.CreateImportDataRequest(ImportMode.CreateAndUpdate);
            var cancellationTokenWrapper = GetCancellationTokenWrapper();
            var progressInfos = new List<ImportProgressInfo>();
            void ProgressCallback(ImportProgressInfo progressInfo)
            {
                progressInfos.Add((ImportProgressInfo)progressInfo.Clone());
            }

            var invalidRows = new[] { row };

            var errorReporterStream = new MemoryStream();

            var importReporterFactoryMock = new Mock<ICsvPriceImportReporterFactory>();
            var importReporterMock = new Mock<ICsvPriceImportReporter>();
            ImportError errorForAssertion = null;

            importReporterMock.Setup(x => x.Write(It.IsAny<ImportError>()))
                .Callback<ImportError>(error => errorForAssertion = error);

            importReporterFactoryMock.Setup(x => x.CreateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(importReporterMock.Object);

            var allRows = validRows.Union(invalidRows).ToArray();

            var importer = GetCsvPagedPriceDataImporter(GetBlobStorageProvider(CsvHeader, allRows, errorReporterStream), importReporterFactoryMock.Object);

            // Act
            await importer.ImportAsync(request, ProgressCallback, cancellationTokenWrapper);

            // Assert
            var errorProgressInfo = progressInfos.LastOrDefault();
            Assert.Equal(validRows.Length + invalidRows.Length, errorProgressInfo?.ProcessedCount);
            Assert.Equal(validRows.Length, errorProgressInfo?.CreatedCount);
            Assert.Equal(0, errorProgressInfo?.UpdatedCount);
            Assert.Equal(invalidRows.Length, errorProgressInfo?.ErrorCount);
            Assert.NotNull(errorProgressInfo?.Description);
            Assert.StartsWith("Import completed with errors", errorProgressInfo?.Description);

            importReporterMock.Verify(x => x.Write(It.IsAny<ImportError>()), Times.Once);

            Assert.Equal("This row has invalid data. The data after field with not escaped quote was lost.", errorForAssertion.Error);
            Assert.Equal($"{invalidRows.First()}", errorForAssertion.RawRow.TrimEnd());
        }

        [Theory]
        [InlineData("\"X\"XX\";1;1;9")]

        public async Task ImportAsync_NotEscapedQuoteInDataDuringImport_WillReportError(string row)
        {
            // Arrange
            var request = TestHelper.CreateImportDataRequest();
            var cancellationTokenWrapper = GetCancellationTokenWrapper();
            var progressInfos = new List<ImportProgressInfo>();
            void ProgressCallback(ImportProgressInfo progressInfo)
            {
                progressInfos.Add((ImportProgressInfo)progressInfo.Clone());
            }

            var invalidRows = new[] { row };

            var errorReporterStream = new MemoryStream();

            var importReporterFactoryMock = new Mock<ICsvPriceImportReporterFactory>();
            var importReporterMock = new Mock<ICsvPriceImportReporter>();
            ImportError errorForAssertion = null;

            importReporterMock.Setup(x => x.Write(It.IsAny<ImportError>()))
                .Callback<ImportError>(error => errorForAssertion = error);

            importReporterFactoryMock.Setup(x => x.CreateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(importReporterMock.Object);

            var allRows = validRows.Union(invalidRows).ToArray();

            var importer = GetCsvPagedPriceDataImporter(GetBlobStorageProvider(CsvHeader, allRows, errorReporterStream), importReporterFactoryMock.Object);

            // Act
            await importer.ImportAsync(request, ProgressCallback, cancellationTokenWrapper);

            // Assert
            var errorProgressInfo = progressInfos.LastOrDefault();
            Assert.Equal(validRows.Length + invalidRows.Length, errorProgressInfo?.ProcessedCount);
            Assert.Equal(validRows.Length, errorProgressInfo?.CreatedCount);
            Assert.Equal(0, errorProgressInfo?.UpdatedCount);
            Assert.Equal(invalidRows.Length, errorProgressInfo?.ErrorCount);
            Assert.NotNull(errorProgressInfo?.Description);
            Assert.StartsWith("Import completed with errors", errorProgressInfo?.Description);

            importReporterMock.Verify(x => x.Write(It.IsAny<ImportError>()), Times.Once);

            Assert.Equal($"This row has invalid data. The data after field with not escaped quote was lost.", errorForAssertion.Error);
        }

        [Theory]
        [InlineData("SKU1;1;10.99", "Sale price")]
        [InlineData("SKU1;1", "List price, Sale price")]
        [InlineData("SKU1", "Min quantity, List price, Sale price")]
        public async Task ImportAsync_MissedColumns_WillReportError(string row, string missingColumns)
        {
            // Arrange
            var request = TestHelper.CreateImportDataRequest();
            var cancellationTokenWrapper = GetCancellationTokenWrapper();
            var progressInfos = new List<ImportProgressInfo>();

            void ProgressCallback(ImportProgressInfo progressInfo)
            {
                progressInfos.Add((ImportProgressInfo)progressInfo.Clone());
            }

            var invalidRows = new[] { row };
            var errorReporterStream = new MemoryStream();

            var importReporterFactoryMock = new Mock<ICsvPriceImportReporterFactory>();
            var importReporterMock = new Mock<ICsvPriceImportReporter>();
            ImportError errorForAssertion = null;

            importReporterMock.Setup(x => x.Write(It.IsAny<ImportError>()))
                .Callback<ImportError>(error => errorForAssertion = error);

            importReporterFactoryMock.Setup(x => x.CreateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(importReporterMock.Object);

            var allRows = validRows.Union(invalidRows).ToArray();
            var importer = GetCsvPagedPriceDataImporter(GetBlobStorageProvider(CsvHeader, allRows, errorReporterStream), importReporterFactoryMock.Object);

            // Act
            await importer.ImportAsync(request, ProgressCallback, cancellationTokenWrapper);

            // Assert
            var errorProgressInfo = progressInfos.LastOrDefault();
            Assert.Equal(validRows.Length + invalidRows.Length, errorProgressInfo?.ProcessedCount);
            Assert.Equal(validRows.Length, errorProgressInfo?.CreatedCount);
            Assert.Equal(0, errorProgressInfo?.UpdatedCount);
            Assert.Equal(invalidRows.Length, errorProgressInfo?.ErrorCount);
            Assert.NotNull(errorProgressInfo?.Description);
            Assert.StartsWith("Import completed with errors", errorProgressInfo?.Description);

            importReporterMock.Verify(x => x.Write(It.IsAny<ImportError>()), Times.Once());

            Assert.Equal($"This row has next missing columns: {missingColumns}.", errorForAssertion.Error);
            Assert.Equal($"{invalidRows.First()}", errorForAssertion.RawRow.TrimEnd());
        }

        [Theory]
        [InlineData("SKU1;;;9", "Min quantity, List price")]
        [InlineData(";;;9", "SKU, Min quantity, List price")]
        public async Task ImportAsync_SeveralRequiredValueMissed_WillReportError(string row, string missedValueColumns)
        {
            // Arrange
            var request = TestHelper.CreateImportDataRequest();
            var cancellationTokenWrapper = GetCancellationTokenWrapper();
            var progressInfos = new List<ImportProgressInfo>();

            void ProgressCallback(ImportProgressInfo progressInfo)
            {
                progressInfos.Add((ImportProgressInfo)progressInfo.Clone());
            }

            var invalidRows = new[] { row };
            var errorReporterStream = new MemoryStream();

            var importReporterFactoryMock = new Mock<ICsvPriceImportReporterFactory>();
            var importReporterMock = new Mock<ICsvPriceImportReporter>();
            ImportError errorForAssertion = null;

            importReporterMock.Setup(x => x.Write(It.IsAny<ImportError>()))
                .Callback<ImportError>(error => errorForAssertion = error);

            importReporterFactoryMock.Setup(x => x.CreateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(importReporterMock.Object);

            var allRows = validRows.Union(invalidRows).ToArray();
            var importer = GetCsvPagedPriceDataImporter(GetBlobStorageProvider(CsvHeader, allRows, errorReporterStream), importReporterFactoryMock.Object);

            // Act
            await importer.ImportAsync(request, ProgressCallback, cancellationTokenWrapper);

            // Assert
            var errorProgressInfo = progressInfos.LastOrDefault();
            Assert.Equal(validRows.Length + invalidRows.Length, errorProgressInfo?.ProcessedCount);
            Assert.Equal(validRows.Length, errorProgressInfo?.CreatedCount);
            Assert.Equal(0, errorProgressInfo?.UpdatedCount);
            Assert.Equal(invalidRows.Length, errorProgressInfo?.ErrorCount);
            Assert.NotNull(errorProgressInfo?.Description);
            Assert.StartsWith("Import completed with errors", errorProgressInfo?.Description);

            importReporterMock.Verify(x => x.Write(It.IsAny<ImportError>()), Times.Once());

            Assert.Equal($"The required values in columns: {missedValueColumns} - are missing.", errorForAssertion.Error);
            Assert.Equal($"{invalidRows.First()}", errorForAssertion.RawRow.TrimEnd());
        }

        [Theory]
        [InlineData("SKU1;1;;9", "List price")]
        [InlineData("SKU1;;10;9", "Min quantity")]
        //[InlineData(";2;10;", "SKU")]
        public async Task ImportAsync_OneRequiredValueMissed_WillReportError(string row, string missingValueColumn)
        {
            // Arrange
            var request = TestHelper.CreateImportDataRequest();
            var cancellationTokenWrapper = GetCancellationTokenWrapper();
            var progressInfos = new List<ImportProgressInfo>();

            void ProgressCallback(ImportProgressInfo progressInfo)
            {
                progressInfos.Add((ImportProgressInfo)progressInfo.Clone());
            }

            var invalidRows = new[] { row };
            var errorReporterStream = new MemoryStream();

            var importReporterFactoryMock = new Mock<ICsvPriceImportReporterFactory>();
            var importReporterMock = new Mock<ICsvPriceImportReporter>();
            ImportError errorForAssertion = null;

            importReporterMock.Setup(x => x.Write(It.IsAny<ImportError>()))
                .Callback<ImportError>(error => errorForAssertion = error);

            importReporterFactoryMock.Setup(x => x.CreateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(importReporterMock.Object);

            var allRows = validRows.Union(invalidRows).ToArray();
            var importer = GetCsvPagedPriceDataImporter(GetBlobStorageProvider(CsvHeader, allRows, errorReporterStream), importReporterFactoryMock.Object);

            // Act
            await importer.ImportAsync(request, ProgressCallback, cancellationTokenWrapper);

            // Assert
            var errorProgressInfo = progressInfos.LastOrDefault();
            Assert.Equal(validRows.Length + invalidRows.Length, errorProgressInfo?.ProcessedCount);
            Assert.Equal(validRows.Length, errorProgressInfo?.CreatedCount);
            Assert.Equal(0, errorProgressInfo?.UpdatedCount);
            Assert.Equal(invalidRows.Length, errorProgressInfo?.ErrorCount);
            Assert.NotNull(errorProgressInfo?.Description);
            Assert.StartsWith("Import completed with errors", errorProgressInfo?.Description);

            importReporterMock.Verify(x => x.Write(It.IsAny<ImportError>()), Times.Once());

            Assert.Equal($"The required value in column {missingValueColumn} is missing.", errorForAssertion.Error);
            Assert.Equal($"{invalidRows.First()}", errorForAssertion.RawRow.TrimEnd());
        }


        private static CancellationTokenWrapper GetCancellationTokenWrapper()
        {
            return new CancellationTokenWrapper(new CancellationToken());
        }

        private static IBlobStorageProvider GetBlobStorageProvider(string header, string[] records, MemoryStream errorReporterMemoryStream = null)
        {
            errorReporterMemoryStream ??= new MemoryStream();
            var blobStorageProviderMock = new Mock<IBlobStorageProvider>();
            blobStorageProviderMock.Setup(x => x.OpenRead(It.IsAny<string>())).Returns(() => TestHelper.GetStream(TestHelper.GetCsv(records, header)));
            blobStorageProviderMock.Setup(x => x.OpenWrite(It.IsAny<string>())).Returns(() => errorReporterMemoryStream);
            blobStorageProviderMock.Setup(x => x.GetBlobInfoAsync(It.IsAny<string>()))
                .Returns(() => Task.FromResult(new BlobInfo { Size = TestHelper.GetStream(TestHelper.GetCsv(records, header)).Length }));
            return blobStorageProviderMock.Object;
        }

        private static IPriceService GetPricingService()
        {
            var pricingServiceMock = new Mock<IPriceService>();
            pricingServiceMock.Setup(x => x.SaveChangesAsync(It.IsAny<Price[]>()));
            return pricingServiceMock.Object;
        }


        private static IPriceSearchService GetPricingSearchService()
        {
            var pricingSearchServiceMock = new Mock<IPriceSearchService>();
            pricingSearchServiceMock.Setup(x => x.SearchAsync(It.IsAny<PricesSearchCriteria>(), It.IsAny<bool>()))
                .Returns(() => Task.FromResult(new PriceSearchResult
                {
                    TotalCount = 2,
                    Results = new List<Price>
                {
                    new Price { PricelistId = "TestId", ProductId = "TestId1", MinQuantity = 1 },
                    new Price { PricelistId = "TestId", ProductId = "TestId2", MinQuantity = 1 }

                }
                }));
            return pricingSearchServiceMock.Object;
        }

        private static ICsvPriceDataValidator GetPriceDataValidator(IBlobStorageProvider blobStorageProvider)
        {
            return new CsvPriceDataValidator(blobStorageProvider, TestHelper.GetSettingsManagerMoq().Object, new ImportConfigurationFactory());
        }

        private static ImportProductPricesValidator GetImportProductPricesValidator(IPriceSearchService pricingSearchService)
        {
            return new ImportProductPricesValidator(pricingSearchService);
        }

        private static CsvPagedPriceDataImporter GetCsvPagedPriceDataImporter(IBlobStorageProvider blobStorageProvider, ICsvPriceImportReporterFactory importReporterFactory = null)
        {
            var blobUrlResolverMock = new Mock<IBlobUrlResolver>();
            blobUrlResolverMock.Setup(x => x.GetAbsoluteUrl(It.IsAny<string>())).Returns("test_path.csv");

            var pricingSearchService = GetPricingSearchService();


            importReporterFactory ??= new CsvPriceImportReporterFactory(blobStorageProvider);
            return new CsvPagedPriceDataImporter(blobStorageProvider, GetPricingService(), pricingSearchService,
                GetPriceDataValidator(blobStorageProvider), TestHelper.GetCsvPagedPriceDataSourceFactory(blobStorageProvider), GetImportProductPricesValidator(pricingSearchService), importReporterFactory, blobUrlResolverMock.Object, new ImportConfigurationFactory());
        }
    }
}
