using System.IO;
using CsvHelper.Configuration;
using VirtoCommerce.SimpleExportImportModule.Core.Models;
using VirtoCommerce.SimpleExportImportModule.Core.Services;

namespace VirtoCommerce.SimpleExportImportModule.Data.Services
{
    public sealed class CsvPriceImportReporterFactory : ICsvPriceImportReporterFactory
    {
        public ICsvPriceImportReporter Create(Stream stream, Configuration configuration = null)
        {
            return new CsvPriceImportReporter(stream, configuration ?? new ImportConfiguration());
        }
    }
}
