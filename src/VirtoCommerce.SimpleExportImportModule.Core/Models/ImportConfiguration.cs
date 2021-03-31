using System.Globalization;
using CsvHelper.Configuration;

namespace VirtoCommerce.SimpleExportImportModule.Core.Models
{
    public sealed class ImportConfiguration : Configuration
    {
        public ImportConfiguration()
            : base(CultureInfo.InvariantCulture)
        {
            Delimiter = ";";
        }
    }
}
