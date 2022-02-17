using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using VirtoCommerce.PriceExportImportModule.Data.Models;
using VirtoCommerce.PriceExportImportModule.Data.Services;
using Xunit;

namespace VirtoCommerce.PriceExportImportModule.Tests
{
    public class CsvHelperTests
    {

        [Fact]
        public async Task TestDoubleBadDataFoundCase()
        {
            var errorCount = 0;
            var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                ReadingExceptionOccurred = args => false,
                BadDataFound = args =>
                {
                    ++errorCount;
                    if (args.Context.Reader is VcCsvReader vcCsvReader)
                    {
                        vcCsvReader.IsFieldBadData = true;
                    }
                },
                Delimiter = ";",
            };
            var header = "SKU;Min quantity;List price;Sale price";
            var records = new[] { "TestSku1;2;10.99;9.99", "TestSku2;2;10.99;9", "XXX;\"9;10.9;9" };
            var csv = TestHelper.GetCsv(records, header);
            var textReader = new StreamReader(TestHelper.GetStream(csv), leaveOpen: true);

            var csvReader = new VcCsvReader(textReader, csvConfiguration);

            await csvReader.ReadAsync();
            csvReader.ReadHeader();

            while (await csvReader.ReadAsync())
            {
                if (!csvReader.IsFieldBadData)
                {
                    csvReader.GetRecord<CsvPrice>();
                }

                csvReader.IsFieldBadData = false;

            }

            Assert.Equal(1, errorCount);
        }

        [Fact]
        public async Task EnsureBadDataFoundWasCalledOnceCase()
        {
            var isRecordBad = false;
            var errorCount = 0;
            var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                //ReadingExceptionOccurred = args => false,
                BadDataFound = args =>
                {
                    ++errorCount;
                    isRecordBad = true;
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
                if (!isRecordBad)
                {
                    csvReader.GetRecord<CsvPrice>();
                }

                isRecordBad = false;
            }

            Assert.Equal(1, errorCount);
        }
    }
}
