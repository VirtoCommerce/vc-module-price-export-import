using System;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace VirtoCommerce.SimpleExportImportModule.Core.Models
{
    public sealed class ImportConfiguration : Configuration
    {
        public ImportConfiguration()
            : base(CultureInfo.InvariantCulture)
        {
        }

        public override string Delimiter { get; set; } = ";";

        public override Func<CsvHelperException, bool> ReadingExceptionOccurred { get; set; } = ex => false;
    }
}
