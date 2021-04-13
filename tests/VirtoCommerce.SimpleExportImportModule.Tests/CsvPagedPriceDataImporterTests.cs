using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper.Configuration;
using Moq;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Model.Search;
using VirtoCommerce.PricingModule.Core.Services;
using VirtoCommerce.SimpleExportImportModule.Core.Models;
using VirtoCommerce.SimpleExportImportModule.Core.Services;
using VirtoCommerce.SimpleExportImportModule.Data.Services;
using VirtoCommerce.SimpleExportImportModule.Data.Validation;
using Xunit;

namespace VirtoCommerce.SimpleExportImportModule.Tests
{
    [Trait("Category", "CI")]
    public class CsvPagedPriceDataImporterTests
    {
        private const string CsvHeader = "SKU;Min quantity;List price;Sale price";
        private const string CsvHeader2 = "SKU;Min quantity;Currency;List price;Sale price;Created by;Modified By";
        private static readonly string[] CsvRecords = { "TestSku1;1;100;99", "TestSku2;1;10;9.99", ";10;9;" };

        [Fact]
        public async Task ImportAsync_CancelledBeforeStart_WillThrowException()
        {
            static async Task ImportAsync()
            {
                // Arrange
                var request = CreateImportDataRequest();
                var cancellationToken = new CancellationToken(true);
                var cancellationTokenWrapper = new CancellationTokenWrapper(cancellationToken);
                var importer = GetCsvPagedPriceDataImporter(GetBlobStorageProvider(CsvHeader, CsvRecords));

                // Act
                await importer.ImportAsync(request, ProgressCallbackMock, cancellationTokenWrapper);
            }

            // Assert
            await Assert.ThrowsAsync<OperationCanceledException>(ImportAsync);
        }

        [Fact]
        public async Task ImportAsync_WithFailedFileValidation_WillThrowException()
        {
            static async Task ImportAsync()
            {
                // Arrange
                var request = CreateImportDataRequest();
                var cancellationTokenWrapper = GetCancellationTokenWrapper();
                var importer = GetCsvPagedPriceDataImporter(GetBlobStorageProvider(null, CsvRecords));

                // Act
                await importer.ImportAsync(request, ProgressCallbackMock, cancellationTokenWrapper);
            }

            // Assert
            await Assert.ThrowsAsync<InvalidDataException>(ImportAsync);
        }

        [Fact]
        public async Task ImportAsync_BeforeImport_WillReportProgressWithTotalCount()
        {
            // Arrange
            var request = CreateImportDataRequest();
            var cancellationTokenWrapper = GetCancellationTokenWrapper();
            var progressInfos = new List<ImportProgressInfo>();

            void ProgressCallback(ImportProgressInfo progressInfo)
            {
                progressInfos.Add((ImportProgressInfo)progressInfo.Clone());
            }

            var importer = GetCsvPagedPriceDataImporter(GetBlobStorageProvider(CsvHeader, CsvRecords));

            // Act
            await importer.ImportAsync(request, ProgressCallback, cancellationTokenWrapper);

            // Assert
            var startProgressInfo = progressInfos.FirstOrDefault();
            Assert.Equal(0, startProgressInfo?.ProcessedCount);
            Assert.Equal(0, startProgressInfo?.CreatedCount);
            Assert.Equal(0, startProgressInfo?.UpdatedCount);
            Assert.Equal(0, startProgressInfo?.ErrorCount);
            Assert.Equal("Import has started", startProgressInfo?.Description);
            Assert.Equal(3, startProgressInfo?.TotalCount);
        }

        [Fact]
        public async Task ImportAsync_StartImport_WillReportStartOfReading()
        {
            // Arrange
            var request = CreateImportDataRequest();
            var cancellationTokenWrapper = GetCancellationTokenWrapper();
            var progressInfos = new List<ImportProgressInfo>();

            void ProgressCallback(ImportProgressInfo progressInfo)
            {
                progressInfos.Add((ImportProgressInfo)progressInfo.Clone());
            }

            var importer = GetCsvPagedPriceDataImporter(GetBlobStorageProvider(CsvHeader, CsvRecords));

            // Act
            await importer.ImportAsync(request, ProgressCallback, cancellationTokenWrapper);

            // Assert
            var startProgressInfo = progressInfos[1];
            Assert.Equal("Fetching...", startProgressInfo?.Description);
        }

        [Fact]
        public async Task ImportAsync_ExceptionDuringImport_WillReportError()
        {
            // Arrange
            var request = CreateImportDataRequest();
            var cancellationTokenWrapper = GetCancellationTokenWrapper();
            var progressInfos = new List<ImportProgressInfo>();

            void ProgressCallback(ImportProgressInfo progressInfo)
            {
                progressInfos.Add((ImportProgressInfo)progressInfo.Clone());
            }

            var invalidRows = new[] { "XXX;Y;Y;Y" };
            var importer = GetCsvPagedPriceDataImporter(GetBlobStorageProvider(CsvHeader, invalidRows));

            // Act
            await importer.ImportAsync(request, ProgressCallback, cancellationTokenWrapper);

            // Assert
            var errorProgressInfo = progressInfos.LastOrDefault();
            Assert.Equal(1, errorProgressInfo?.ProcessedCount);
            Assert.Equal(0, errorProgressInfo?.CreatedCount);
            Assert.Equal(0, errorProgressInfo?.UpdatedCount);
            Assert.Equal(1, errorProgressInfo?.ErrorCount);
            Assert.NotNull(errorProgressInfo?.Description);
            Assert.StartsWith("Import completed with errors", errorProgressInfo?.Description);
        }

        [Theory]
        [InlineData("XXX;Text;10.9;Text", "Min quantity")]
        [InlineData("\"XXX\";2;Text;9.9", "List price")]
        [InlineData("SKU1;1;10.9;Text", "Sale price")]
        public async Task ImportAsync_InvalidColumnValueDuringImport_WillReportError(string row, string invalidFieldName)
        {
            // Arrange
            var request = CreateImportDataRequest();
            var cancellationTokenWrapper = GetCancellationTokenWrapper();
            var progressInfos = new List<ImportProgressInfo>();
            void ProgressCallback(ImportProgressInfo progressInfo)
            {
                progressInfos.Add((ImportProgressInfo)progressInfo.Clone());
            }

            var validRows = new[] { "SKU1;1;10.99;9.99", "SKU2;1;10.99;9" };
            var invalidRows = new[] { row };

            var errorReporterStream = new MemoryStream();

            var importReporterFactoryMock = new Mock<ICsvPriceImportReporterFactory>();
            var importReporterMock = new Mock<ICsvPriceImportReporter>();
            ImportError errorForAssertion = null;

            importReporterMock.Setup(x => x.WriteAsync(It.IsAny<ImportError>()))
                .Callback<ImportError>(error => errorForAssertion = error)
                .Returns(Task.CompletedTask);

            importReporterFactoryMock.Setup(x => x.Create(It.IsAny<Stream>(), It.IsAny<Configuration>()))
                .Returns(importReporterMock.Object);

            var allRows = validRows.Union(invalidRows).ToArray();

            var importer = GetCsvPagedPriceDataImporter(GetBlobStorageProvider(CsvHeader, allRows, errorReporterStream), importReporterFactoryMock.Object);

            // Act
            await importer.ImportAsync(request, ProgressCallback, cancellationTokenWrapper);

            // Assert
            var errorProgressInfo = progressInfos.LastOrDefault();
            Assert.Equal(validRows.Length + invalidRows.Length, errorProgressInfo?.ProcessedCount);
            Assert.Equal(0, errorProgressInfo?.CreatedCount);
            Assert.Equal(0, errorProgressInfo?.UpdatedCount);
            Assert.Equal(validRows.Length + invalidRows.Length, errorProgressInfo?.ErrorCount);
            Assert.NotNull(errorProgressInfo?.Description);
            Assert.StartsWith("Import completed with errors", errorProgressInfo?.Description);

            importReporterMock.Verify(x => x.WriteAsync(It.IsAny<ImportError>()), Times.Once);

            Assert.Equal($"This row has invalid value in the column {invalidFieldName}", errorForAssertion.Error);
            Assert.Equal($"{invalidRows.First()}\r\n", errorForAssertion.RawRow);
        }


        [Theory]
        [InlineData("XXX;\"9;10.9;9")]
        [InlineData("\"XXX\";2;\"9;9.9")]
        public async Task ImportAsync_BadDataFoundDuringImport_WillReportError(string row)
        {
            // Arrange
            var request = CreateImportDataRequest();
            var cancellationTokenWrapper = GetCancellationTokenWrapper();
            var progressInfos = new List<ImportProgressInfo>();
            void ProgressCallback(ImportProgressInfo progressInfo)
            {
                progressInfos.Add((ImportProgressInfo)progressInfo.Clone());
            }

            var validRows = new[] { "SKU1;1;10.99;9.99", "SKU2;1;10.99;9" };
            var invalidRows = new[] { row };

            var errorReporterStream = new MemoryStream();

            var importReporterFactoryMock = new Mock<ICsvPriceImportReporterFactory>();
            var importReporterMock = new Mock<ICsvPriceImportReporter>();
            ImportError errorForAssertion = null;

            importReporterMock.Setup(x => x.WriteAsync(It.IsAny<ImportError>()))
                .Callback<ImportError>(error => errorForAssertion = error)
                .Returns(Task.CompletedTask);

            importReporterFactoryMock.Setup(x => x.Create(It.IsAny<Stream>(), It.IsAny<Configuration>()))
                .Returns(importReporterMock.Object);

            var allRows = validRows.Union(invalidRows).ToArray();

            var importer = GetCsvPagedPriceDataImporter(GetBlobStorageProvider(CsvHeader, allRows, errorReporterStream), importReporterFactoryMock.Object);

            // Act
            await importer.ImportAsync(request, ProgressCallback, cancellationTokenWrapper);

            // Assert
            var errorProgressInfo = progressInfos.LastOrDefault();
            Assert.Equal(validRows.Length + invalidRows.Length, errorProgressInfo?.ProcessedCount);
            Assert.Equal(0, errorProgressInfo?.CreatedCount);
            Assert.Equal(0, errorProgressInfo?.UpdatedCount);
            Assert.Equal(validRows.Length + invalidRows.Length, errorProgressInfo?.ErrorCount);
            Assert.NotNull(errorProgressInfo?.Description);
            Assert.StartsWith("Import completed with errors", errorProgressInfo?.Description);

            importReporterMock.Verify(x => x.WriteAsync(It.IsAny<ImportError>()), Times.Once);

            Assert.Equal($"This row has invalid data. Quotes should be closed", errorForAssertion.Error);
            Assert.Equal($"{invalidRows.First()}\r\n", errorForAssertion.RawRow);
        }

        [Theory]
        [InlineData("\"X\"XX\";1;1;9")]

        public async Task ImportAsync_NotEscapedQuoteInDataDuringImport_WillReportError(string row)
        {
            // Arrange
            var request = CreateImportDataRequest();
            var cancellationTokenWrapper = GetCancellationTokenWrapper();
            var progressInfos = new List<ImportProgressInfo>();
            void ProgressCallback(ImportProgressInfo progressInfo)
            {
                progressInfos.Add((ImportProgressInfo)progressInfo.Clone());
            }

            var validRows = new[] { "SKU1;1;10.99;9.99", "SKU2;1;10.99;9" };
            var invalidRows = new[] { row };

            var errorReporterStream = new MemoryStream();

            var importReporterFactoryMock = new Mock<ICsvPriceImportReporterFactory>();
            var importReporterMock = new Mock<ICsvPriceImportReporter>();
            ImportError errorForAssertion = null;

            importReporterMock.Setup(x => x.WriteAsync(It.IsAny<ImportError>()))
                .Callback<ImportError>(error => errorForAssertion = error)
                .Returns(Task.CompletedTask);

            importReporterFactoryMock.Setup(x => x.Create(It.IsAny<Stream>(), It.IsAny<Configuration>()))
                .Returns(importReporterMock.Object);

            var allRows = validRows.Union(invalidRows).ToArray();

            var importer = GetCsvPagedPriceDataImporter(GetBlobStorageProvider(CsvHeader, allRows, errorReporterStream), importReporterFactoryMock.Object);

            // Act
            await importer.ImportAsync(request, ProgressCallback, cancellationTokenWrapper);

            // Assert
            var errorProgressInfo = progressInfos.LastOrDefault();
            Assert.Equal(validRows.Length + invalidRows.Length, errorProgressInfo?.ProcessedCount);
            Assert.Equal(0, errorProgressInfo?.CreatedCount);
            Assert.Equal(0, errorProgressInfo?.UpdatedCount);
            Assert.Equal(validRows.Length + invalidRows.Length, errorProgressInfo?.ErrorCount);
            Assert.NotNull(errorProgressInfo?.Description);
            Assert.StartsWith("Import completed with errors", errorProgressInfo?.Description);

            importReporterMock.Verify(x => x.WriteAsync(It.IsAny<ImportError>()), Times.Once);

            Assert.Equal($"This row has invalid data. The data after field with not escaped quote was lost", errorForAssertion.Error);
            //Assert.Equal($"{invalidRows.First()}\r\n", errorForAssertion.RawRow);
        }

        [Theory]
        [InlineData("SKU1;1;10.99", "Sale price")]
        [InlineData("SKU1;1", "List price, Sale price")]
        [InlineData("SKU1", "Min quantity, List price, Sale price")]
        public async Task ImportAsync_MissedColumns_WillReportError(string row, string missingColumns)
        {
            // Arrange
            var request = CreateImportDataRequest();
            var cancellationTokenWrapper = GetCancellationTokenWrapper();
            var progressInfos = new List<ImportProgressInfo>();

            void ProgressCallback(ImportProgressInfo progressInfo)
            {
                progressInfos.Add((ImportProgressInfo)progressInfo.Clone());
            }

            var validRows = new[] { "SKU1;1;10.99;9.99", "SKU2;1;10.99;9" };
            var invalidRows = new[] { row };
            var errorReporterStream = new MemoryStream();

            var importReporterFactoryMock = new Mock<ICsvPriceImportReporterFactory>();
            var importReporterMock = new Mock<ICsvPriceImportReporter>();
            ImportError errorForAssertion = null;

            importReporterMock.Setup(x => x.WriteAsync(It.IsAny<ImportError>()))
                .Callback<ImportError>(error => errorForAssertion = error);

            importReporterFactoryMock.Setup(x => x.Create(It.IsAny<Stream>(), It.IsAny<Configuration>()))
                .Returns(importReporterMock.Object);

            var allRows = validRows.Union(invalidRows).ToArray();
            var importer = GetCsvPagedPriceDataImporter(GetBlobStorageProvider(CsvHeader, allRows, errorReporterStream), importReporterFactoryMock.Object);

            // Act
            await importer.ImportAsync(request, ProgressCallback, cancellationTokenWrapper);

            // Assert
            var errorProgressInfo = progressInfos.LastOrDefault();
            Assert.Equal(validRows.Length + invalidRows.Length, errorProgressInfo?.ProcessedCount);
            Assert.Equal(0, errorProgressInfo?.CreatedCount);
            Assert.Equal(0, errorProgressInfo?.UpdatedCount);
            Assert.Equal(validRows.Length + invalidRows.Length, errorProgressInfo?.ErrorCount);
            Assert.NotNull(errorProgressInfo?.Description);
            Assert.StartsWith("Import completed with errors", errorProgressInfo?.Description);

            importReporterMock.Verify(x => x.WriteAsync(It.IsAny<ImportError>()), Times.Once());

            Assert.Equal($"This row has next missing columns: {missingColumns}", errorForAssertion.Error);
            Assert.Equal($"{invalidRows.First()}\r\n", errorForAssertion.RawRow);
        }

        [Theory]
        [InlineData("SKU1;;;9", "Min quantity, List price")]
        [InlineData(";;;9", "SKU, Min quantity, List price")]
        public async Task ImportAsync_SeveralRequiredValueMissed_WillReportError(string row, string missedValueColumns)
        {
            // Arrange
            var request = CreateImportDataRequest();
            var cancellationTokenWrapper = GetCancellationTokenWrapper();
            var progressInfos = new List<ImportProgressInfo>();

            void ProgressCallback(ImportProgressInfo progressInfo)
            {
                progressInfos.Add((ImportProgressInfo)progressInfo.Clone());
            }

            var validRows = new[] { "SKU1;1;10.99;9.99", "SKU2;1;10.99;9" };
            var invalidRows = new[] { row };
            var errorReporterStream = new MemoryStream();

            var importReporterFactoryMock = new Mock<ICsvPriceImportReporterFactory>();
            var importReporterMock = new Mock<ICsvPriceImportReporter>();
            ImportError errorForAssertion = null;

            importReporterMock.Setup(x => x.WriteAsync(It.IsAny<ImportError>()))
                .Callback<ImportError>(error => errorForAssertion = error);

            importReporterFactoryMock.Setup(x => x.Create(It.IsAny<Stream>(), It.IsAny<Configuration>()))
                .Returns(importReporterMock.Object);

            var allRows = validRows.Union(invalidRows).ToArray();
            var importer = GetCsvPagedPriceDataImporter(GetBlobStorageProvider(CsvHeader, allRows, errorReporterStream), importReporterFactoryMock.Object);

            // Act
            await importer.ImportAsync(request, ProgressCallback, cancellationTokenWrapper);

            // Assert
            var errorProgressInfo = progressInfos.LastOrDefault();
            Assert.Equal(validRows.Length + invalidRows.Length, errorProgressInfo?.ProcessedCount);
            Assert.Equal(0, errorProgressInfo?.CreatedCount);
            Assert.Equal(0, errorProgressInfo?.UpdatedCount);
            Assert.Equal(validRows.Length + invalidRows.Length, errorProgressInfo?.ErrorCount);
            Assert.NotNull(errorProgressInfo?.Description);
            Assert.StartsWith("Import completed with errors", errorProgressInfo?.Description);

            importReporterMock.Verify(x => x.WriteAsync(It.IsAny<ImportError>()), Times.Once());

            Assert.Equal($"The required values in columns: {missedValueColumns} - are missing", errorForAssertion.Error);
            Assert.Equal($"{invalidRows.First()}\r\n", errorForAssertion.RawRow);
        }

        [Theory]
        [InlineData("SKU1;1;;9", "List price")]
        [InlineData("SKU1;;10;9", "Min quantity")]
        //[InlineData(";2;10;", "SKU")]
        public async Task ImportAsync_OneRequiredValueMissed_WillReportError(string row, string missingValueColumn)
        {
            // Arrange
            var request = CreateImportDataRequest();
            var cancellationTokenWrapper = GetCancellationTokenWrapper();
            var progressInfos = new List<ImportProgressInfo>();

            void ProgressCallback(ImportProgressInfo progressInfo)
            {
                progressInfos.Add((ImportProgressInfo)progressInfo.Clone());
            }

            var validRows = new[] { "SKU1;1;10.99;9.99", "SKU2;1;10.99;9" };
            var invalidRows = new[] { row };
            var errorReporterStream = new MemoryStream();

            var importReporterFactoryMock = new Mock<ICsvPriceImportReporterFactory>();
            var importReporterMock = new Mock<ICsvPriceImportReporter>();
            ImportError errorForAssertion = null;

            importReporterMock.Setup(x => x.WriteAsync(It.IsAny<ImportError>()))
                .Callback<ImportError>(error => errorForAssertion = error);

            importReporterFactoryMock.Setup(x => x.Create(It.IsAny<Stream>(), It.IsAny<Configuration>()))
                .Returns(importReporterMock.Object);

            var allRows = validRows.Union(invalidRows).ToArray();
            var importer = GetCsvPagedPriceDataImporter(GetBlobStorageProvider(CsvHeader, allRows, errorReporterStream), importReporterFactoryMock.Object);

            // Act
            await importer.ImportAsync(request, ProgressCallback, cancellationTokenWrapper);

            // Assert
            var errorProgressInfo = progressInfos.LastOrDefault();
            Assert.Equal(validRows.Length + invalidRows.Length, errorProgressInfo?.ProcessedCount);
            Assert.Equal(0, errorProgressInfo?.CreatedCount);
            Assert.Equal(0, errorProgressInfo?.UpdatedCount);
            Assert.Equal(validRows.Length + invalidRows.Length, errorProgressInfo?.ErrorCount);
            Assert.NotNull(errorProgressInfo?.Description);
            Assert.StartsWith("Import completed with errors", errorProgressInfo?.Description);

            importReporterMock.Verify(x => x.WriteAsync(It.IsAny<ImportError>()), Times.Once());

            Assert.Equal($"The required value in column {missingValueColumn} is missing", errorForAssertion.Error);
            Assert.Equal($"{invalidRows.First()}\r\n", errorForAssertion.RawRow);
        }


        [Fact]
        public async Task ImportAsync_IfImportCancelled_WillStop()
        {
            // Arrange
            var request = CreateImportDataRequest();
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var cancellationTokenWrapper = new CancellationTokenWrapper(cancellationToken);
            var progressInfos = new List<ImportProgressInfo>();

            void ProgressCallback(ImportProgressInfo progressInfo)
            {
                if (progressInfo.Description == "Fetching...")
                {
                    cancellationTokenSource.Cancel();
                }

                progressInfos.Add((ImportProgressInfo)progressInfo.Clone());
            }

            var importer = GetCsvPagedPriceDataImporter(GetBlobStorageProvider(CsvHeader, CsvRecords));

            // Act
            await importer.ImportAsync(request, ProgressCallback, cancellationTokenWrapper);

            // Assert
            var errorProgressInfo = progressInfos.LastOrDefault();
            Assert.Equal(0, errorProgressInfo?.ProcessedCount);
            Assert.Equal(0, errorProgressInfo?.CreatedCount);
            Assert.Equal(0, errorProgressInfo?.UpdatedCount);
            Assert.Equal(1, errorProgressInfo?.ErrorCount);
            Assert.NotNull(errorProgressInfo?.Description);
            Assert.StartsWith("Import completed with errors", errorProgressInfo.Description);
        }

        [Fact]
        public async Task ImportAsync_CreateValidPrices_WillReportSuccess()
        {
            // Arrange
            var request = CreateImportDataRequest();
            var cancellationTokenWrapper = GetCancellationTokenWrapper();
            var progressInfos = new List<ImportProgressInfo>();

            void ProgressCallback(ImportProgressInfo progressInfo)
            {
                progressInfos.Add((ImportProgressInfo)progressInfo.Clone());
            }

            var importer = GetCsvPagedPriceDataImporter(GetBlobStorageProvider(CsvHeader, new[] { CsvRecords.First() }));

            // Act
            await importer.ImportAsync(request, ProgressCallback, cancellationTokenWrapper);

            // Assert
            var successProgressInfo = progressInfos.LastOrDefault();
            Assert.Equal(1, successProgressInfo?.ProcessedCount);
            Assert.Equal(1, successProgressInfo?.CreatedCount);
            Assert.Equal(0, successProgressInfo?.UpdatedCount);
            Assert.Equal(0, successProgressInfo?.ErrorCount);
            Assert.NotNull(successProgressInfo?.Description);
            Assert.StartsWith("Import completed", successProgressInfo.Description);
            Assert.DoesNotContain("error", successProgressInfo.Description);
        }

        [Fact]
        public async Task ImportAsync_CreateInvalidPrices_WillIgnore()
        {
            // Arrange
            var request = CreateImportDataRequest();
            var cancellationTokenWrapper = GetCancellationTokenWrapper();
            var progressInfos = new List<ImportProgressInfo>();

            void ProgressCallback(ImportProgressInfo progressInfo)
            {
                progressInfos.Add((ImportProgressInfo)progressInfo.Clone());
            }

            var importer = GetCsvPagedPriceDataImporter(GetBlobStorageProvider(CsvHeader, CsvRecords));

            // Act
            await importer.ImportAsync(request, ProgressCallback, cancellationTokenWrapper);

            // Assert
            var successProgressInfo = progressInfos.LastOrDefault();
            Assert.Equal(3, successProgressInfo?.ProcessedCount);
            Assert.Equal(1, successProgressInfo?.CreatedCount);
            Assert.Equal(0, successProgressInfo?.UpdatedCount);
            Assert.Equal(2, successProgressInfo?.ErrorCount);
            Assert.NotNull(successProgressInfo?.Description);
            Assert.StartsWith("Import completed with errors", successProgressInfo.Description);
        }

        [Fact]
        public async Task ImportAsync_UpdateValidPrices_WillReportSuccess()
        {
            // Arrange
            var request = CreateImportDataRequest(ImportMode.UpdateOnly);
            var cancellationTokenWrapper = GetCancellationTokenWrapper();
            var progressInfos = new List<ImportProgressInfo>();

            void ProgressCallback(ImportProgressInfo progressInfo)
            {
                progressInfos.Add((ImportProgressInfo)progressInfo.Clone());
            }

            var importer = GetCsvPagedPriceDataImporter(GetBlobStorageProvider(CsvHeader, new[] { CsvRecords[1] }));

            // Act
            await importer.ImportAsync(request, ProgressCallback, cancellationTokenWrapper);

            // Assert
            var successProgressInfo = progressInfos.LastOrDefault();
            Assert.Equal(1, successProgressInfo?.ProcessedCount);
            Assert.Equal(0, successProgressInfo?.CreatedCount);
            Assert.Equal(1, successProgressInfo?.UpdatedCount);
            Assert.Equal(0, successProgressInfo?.ErrorCount);
            Assert.NotNull(successProgressInfo?.Description);
            Assert.StartsWith("Import completed", successProgressInfo.Description);
            Assert.DoesNotContain("error", successProgressInfo.Description);
        }

        [Fact]
        public async Task ImportAsync_CreateAndUpdateValidPrices_WillReportSuccess()
        {
            // Arrange
            var request = CreateImportDataRequest(ImportMode.CreateAndUpdate);
            var cancellationTokenWrapper = GetCancellationTokenWrapper();
            var progressInfos = new List<ImportProgressInfo>();

            void ProgressCallback(ImportProgressInfo progressInfo)
            {
                progressInfos.Add((ImportProgressInfo)progressInfo.Clone());
            }

            var importer = GetCsvPagedPriceDataImporter(GetBlobStorageProvider(CsvHeader, CsvRecords.Take(2).ToArray()));

            // Act
            await importer.ImportAsync(request, ProgressCallback, cancellationTokenWrapper);

            // Assert
            var successProgressInfo = progressInfos.LastOrDefault();
            Assert.Equal(2, successProgressInfo?.ProcessedCount);
            Assert.Equal(1, successProgressInfo?.CreatedCount);
            Assert.Equal(1, successProgressInfo?.UpdatedCount);
            Assert.Equal(0, successProgressInfo?.ErrorCount);
            Assert.NotNull(successProgressInfo?.Description);
            Assert.StartsWith("Import completed", successProgressInfo.Description);
            Assert.DoesNotContain("error", successProgressInfo.Description);
        }

        private static ImportDataRequest CreateImportDataRequest(ImportMode importMode = ImportMode.CreateOnly)
        {
            return new ImportDataRequest { FileUrl = "https://localhost/test_url.csv", ImportMode = importMode, PricelistId = "TestId" };
        }

        private static CancellationTokenWrapper GetCancellationTokenWrapper()
        {
            return new CancellationTokenWrapper(new CancellationToken());
        }

        private static void ProgressCallbackMock(ImportProgressInfo progressInfo)
        {
            // Mock
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

        private static IPricingService GetPricingService()
        {
            var pricingServiceMock = new Mock<IPricingService>();
            pricingServiceMock.Setup(x => x.SavePricesAsync(It.IsAny<Price[]>()));
            return pricingServiceMock.Object;
        }

        private static IPricingSearchService GetPricingSearchService()
        {
            var pricingSearchServiceMock = new Mock<IPricingSearchService>();
            pricingSearchServiceMock.Setup(x => x.SearchPricesAsync(It.IsAny<PricesSearchCriteria>()))
                .Returns(() => Task.FromResult(new PriceSearchResult { TotalCount = 1, Results = new List<Price> { new Price { PricelistId = "TestId", ProductId = "TestId2", MinQuantity = 1 }, } }));
            return pricingSearchServiceMock.Object;
        }

        private static ICsvPriceDataValidator GetPriceDataValidator(IBlobStorageProvider blobStorageProvider)
        {
            return new CsvPriceDataValidator(blobStorageProvider, TestHelper.GetSettingsManagerMoq().Object);
        }

        private static ImportProductPricesValidator GetImportProductPricesValidator(IPricingSearchService pricingSearchService)
        {
            return new ImportProductPricesValidator(pricingSearchService);
        }

        private static CsvPagedPriceDataImporter GetCsvPagedPriceDataImporter(IBlobStorageProvider blobStorageProvider, ICsvPriceImportReporterFactory importReporterFactory = null)
        {
            var pricingSearchService = GetPricingSearchService();
            importReporterFactory ??= new CsvPriceImportReporterFactory();
            return new CsvPagedPriceDataImporter(blobStorageProvider, GetPricingService(), pricingSearchService,
                GetPriceDataValidator(blobStorageProvider), TestHelper.GetCsvPagedPriceDataSourceFactory(), GetImportProductPricesValidator(pricingSearchService), importReporterFactory, null);
        }
    }
}
