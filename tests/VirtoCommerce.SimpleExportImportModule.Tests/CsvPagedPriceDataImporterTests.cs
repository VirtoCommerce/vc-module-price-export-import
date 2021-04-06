using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private static readonly string[] CsvRecords = { "TestSku1;1;100;99", "TestSku2;1;10;9.99", ";10;9;" };

        [Fact]
        public async Task ImportAsync_CancelledBeforeStart_WillThrowException()
        {
            static async Task ImportAsync()
            {
                // Arrange
                var request = new ImportDataRequest { ImportMode = ImportMode.CreateOnly, PricelistId = "TestId" };
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
                var request = new ImportDataRequest { ImportMode = ImportMode.CreateOnly, PricelistId = "TestId" };
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
            var request = new ImportDataRequest { ImportMode = ImportMode.CreateOnly, PricelistId = "TestId" };
            var cancellationTokenWrapper = GetCancellationTokenWrapper();
            var progressInfos = new List<ImportProgressInfo>();
            void ProgressCallback(ImportProgressInfo progressInfo)
            {
                progressInfos.Add((ImportProgressInfo) progressInfo.Clone());
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
            var request = new ImportDataRequest { ImportMode = ImportMode.CreateOnly, PricelistId = "TestId" };
            var cancellationTokenWrapper = GetCancellationTokenWrapper();
            var progressInfos = new List<ImportProgressInfo>();
            void ProgressCallback(ImportProgressInfo progressInfo)
            {
                progressInfos.Add((ImportProgressInfo) progressInfo.Clone());
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
            var request = new ImportDataRequest { ImportMode = ImportMode.CreateOnly, PricelistId = "TestId" };
            var cancellationTokenWrapper = GetCancellationTokenWrapper();
            var progressInfos = new List<ImportProgressInfo>();
            void ProgressCallback(ImportProgressInfo progressInfo)
            {
                progressInfos.Add((ImportProgressInfo) progressInfo.Clone());
            }
            var invalidRows = new [] { "XXX;Y;Y;Y" };
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

        [Fact]
        public async Task ImportAsync_IfImportCancelled_WillStop()
        {
            // Arrange
            var request = new ImportDataRequest { ImportMode = ImportMode.CreateOnly, PricelistId = "TestId" };
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
                progressInfos.Add((ImportProgressInfo) progressInfo.Clone());
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
            Assert.StartsWith("Import completed with errors", errorProgressInfo?.Description);
        }

        private static CancellationTokenWrapper GetCancellationTokenWrapper()
        {
            return new CancellationTokenWrapper(new CancellationToken());
        }

        private static void ProgressCallbackMock(ImportProgressInfo progressInfo)
        {
            // Mock
        }

        private static IBlobStorageProvider GetBlobStorageProvider(string header, string[] records)
        {
            var blobStorageProviderMock = new Mock<IBlobStorageProvider>();
            blobStorageProviderMock.Setup(x => x.OpenRead(It.IsAny<string>())).Returns(() => TestHelper.GetStream(TestHelper.GetCsv(records, header)));
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
                .Returns(() => Task.FromResult(new PriceSearchResult
                {
                    TotalCount = 1,
                    Results = new List<Price>
                    {
                        new Price
                        {
                            PricelistId = "TestId1",
                            ProductId = "TestId1",
                            MinQuantity = 1
                        },
                    }
                }));
            return pricingSearchServiceMock.Object;
        }

        private static ICsvPriceDataValidator GetPriceDataValidator(IBlobStorageProvider blobStorageProvider)
        {
            return new CsvPriceDataValidator(blobStorageProvider);
        }

        private static ImportProductPricesValidator GetImportProductPricesValidator()
        {
            return new ImportProductPricesValidator(GetPricingSearchService());
        }

        private static CsvPagedPriceDataImporter GetCsvPagedPriceDataImporter(IBlobStorageProvider blobStorageProvider)
        {
            return new CsvPagedPriceDataImporter(GetPricingService(), TestHelper.GetCsvPagedPriceDataSourceFactory(), GetImportProductPricesValidator(), blobStorageProvider, GetPriceDataValidator(blobStorageProvider));
        }
    }
}
