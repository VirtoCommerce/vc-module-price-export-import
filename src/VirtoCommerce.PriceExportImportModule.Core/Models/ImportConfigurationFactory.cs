using System.Globalization;
using CsvHelper.Configuration;

namespace VirtoCommerce.PriceExportImportModule.Core.Models
{
    public class ImportConfigurationFactory
    {
        public virtual CsvConfiguration Create()
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
