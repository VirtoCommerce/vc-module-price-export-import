using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SimpleExportImportModule.Core;
using VirtoCommerce.SimpleExportImportModule.Core.Models;
using VirtoCommerce.SimpleExportImportModule.Data.Services;

namespace VirtoCommerce.SimpleExportImportModule.Tests
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

        public static CsvPagedPriceDataSourceFactory GetCsvPagedPriceDataSourceFactory()
        {
            return new CsvPagedPriceDataSourceFactory(GetProductSearchService());
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
            return new ImportDataRequest { FileUrl = "https://localhost/test_url.csv", ImportMode = importMode, PricelistId = "TestId" };
        }

    }
}
