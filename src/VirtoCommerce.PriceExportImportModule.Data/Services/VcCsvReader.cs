using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;

namespace VirtoCommerce.PriceExportImportModule.Data.Services
{
    public class VcCsvReader : CsvReader
    {
        public bool IsFieldBadData { get; set; }

        public VcCsvReader(TextReader reader, CultureInfo culture, bool leaveOpen = false)
            : base(reader, culture, leaveOpen)
        {
        }

        public VcCsvReader(TextReader reader, CsvConfiguration configuration)
            : base(reader, configuration)
        {
        }

        public VcCsvReader(IParser parser)
            : base(parser)
        {
        }
    }
}
