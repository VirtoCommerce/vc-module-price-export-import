using System.IO;
using CsvHelper.Configuration;

namespace VirtoCommerce.SimpleExportImportModule.Core.Services
{
    public interface ICsvPriceImportReporterFactory
    {
        ICsvPriceImportReporter Create(Stream stream, Configuration configuration = null);
    }
}
