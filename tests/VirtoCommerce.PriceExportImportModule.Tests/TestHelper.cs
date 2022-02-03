using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Moq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.PriceExportImportModule.Core;
using VirtoCommerce.PriceExportImportModule.Core.Models;
using VirtoCommerce.PriceExportImportModule.Data.Services;

namespace VirtoCommerce.PriceExportImportModule.Tests
{
    public static class TestHelper
    {
        private static IProductSearchService GetProductSearchService()
        {
            var productSearchServiceMock = new Mock<IProductSearchService>();
            productSearchServiceMock.Setup(service => service.SearchProductsAsync(It.IsAny<ProductSearchCriteria>()))
                .Returns(() => Task.FromResult(new ProductSearchResult
                {
                    Results = new[]
                    {
                        new CatalogProduct { Id = "TestId1", Code = "TestSku1" },
                        new CatalogProduct { Id = "TestId2", Code = "TestSku2" }
                    },
                    TotalCount = 2
                }));
            return productSearchServiceMock.Object;
        }

        public static IBlobStorageProvider GetBlobStorageProvider(string csv, MemoryStream errorReporterMemoryStream = null)
        {
            errorReporterMemoryStream ??= new MemoryStream();
            var blobStorageProviderMock = new Mock<IBlobStorageProvider>();
            var stream = GetStream(csv);
            blobStorageProviderMock.Setup(x => x.OpenRead(It.IsAny<string>())).Returns(() => stream);
            blobStorageProviderMock.Setup(x => x.OpenWrite(It.IsAny<string>())).Returns(() => errorReporterMemoryStream);
            blobStorageProviderMock.Setup(x => x.GetBlobInfoAsync(It.IsAny<string>()))
                .Returns(() => Task.FromResult(new BlobInfo { Size = stream.Length }));
            return blobStorageProviderMock.Object;
        }

        public static CsvPagedPriceDataSourceFactory GetCsvPagedPriceDataSourceFactory(IBlobStorageProvider blobStorageProvider)
        {
            return new CsvPagedPriceDataSourceFactory(blobStorageProvider, GetProductSearchService(), new ImportConfigurationFactory(), GetCsvReaderFactory());
        }

        public static Func<TextReader, CsvConfiguration, IReader> GetCsvReaderFactory()
        {
            IReader Result(TextReader reader, CsvConfiguration configuration) => new VcCsvReader(reader, configuration);

            return Result;
        }

        public static Stream GetStream(string csv)
        {
            var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, leaveOpen: true);
            writer.Write(csv);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static string GetCsv(IEnumerable<string> records, string header = null)
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

        public static string[] GetArrayOfSameRecords(string recordValue, long number)
        {
            var result = new List<string>();

            for (long i = 0; i < number; i++)
            {
                result.Add(recordValue);
            }

            return result.ToArray();
        }

        public static Mock<ISettingsManager> GetSettingsManagerMoq()
        {
            var settingsManagerMoq = new Mock<ISettingsManager>();

            settingsManagerMoq.Setup(x =>
                    x.GetObjectSettingAsync(
                        It.Is<string>(x => x == ModuleConstants.Settings.General.ImportFileMaxSize.Name),
                        null, null))
                .ReturnsAsync(new ObjectSettingEntry()
                { Value = ModuleConstants.Settings.General.ImportFileMaxSize.DefaultValue });

            settingsManagerMoq.Setup(x =>
                    x.GetObjectSettingAsync(
                        It.Is<string>(x => x == ModuleConstants.Settings.General.ImportLimitOfLines.Name),
                        null, null))
                .ReturnsAsync(new ObjectSettingEntry()
                { Value = ModuleConstants.Settings.General.ImportLimitOfLines.DefaultValue });
            return settingsManagerMoq;
        }

        public static ImportDataRequest CreateImportDataRequest(ImportMode importMode = ImportMode.CreateOnly)
        {
            return new ImportDataRequest { FilePath = "https://localhost/test_url.csv", ImportMode = importMode, PricelistId = "TestId" };
        }

    }
}
