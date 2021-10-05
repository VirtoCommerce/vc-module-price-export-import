using System.Globalization;
using CsvHelper.Configuration;

namespace VirtoCommerce.PriceExportImportModule.Core.Models
{
    public static class ImportConfiguration
    {


        public static CsvConfiguration GetCsvConfiguration()
        {
            var result = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                ReadingExceptionOccurred = args => false,
                BadDataFound = null,
                MissingFieldFound = null,
            };

            return result;
        }

    }
}
