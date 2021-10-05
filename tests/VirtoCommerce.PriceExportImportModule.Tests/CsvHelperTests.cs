using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using VirtoCommerce.PriceExportImportModule.Data.Models;
using Xunit;

namespace VirtoCommerce.PriceExportImportModule.Tests
{
    public class CsvHelperTests
    {

        [Fact]
        public async Task TestDoubleBadDataFoundCase()
        {
            var erroCount = 0;
            var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                ReadingExceptionOccurred = args => false,
                BadDataFound = args =>
                {
                    ++erroCount;
                },
                Delimiter = ";",
            };
            var header = "SKU;Min quantity;List price;Sale price";
            var records = new[] { "TestSku1;2;10.99;9.99", "TestSku2;2;10.99;9", "XXX;\"9;10.9;9" };
            var csv = TestHelper.GetCsv(records, header);
            var textReader = new StreamReader(TestHelper.GetStream(csv), leaveOpen: true);

            var csvReader = new CsvReader(textReader, csvConfiguration);

            while (await csvReader.ReadAsync())
            {
                csvReader.GetRecord<CsvPrice>();
            }

            Assert.Equal(1, erroCount);
        }
    }
}
