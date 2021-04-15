using System.IO;
using System.Threading.Tasks;
using CsvHelper.Configuration;
using VirtoCommerce.SimpleExportImportModule.Core.Models;
using VirtoCommerce.SimpleExportImportModule.Core.Services;

namespace VirtoCommerce.SimpleExportImportModule.Data.Services
{
    public sealed class CsvPriceImportReporter : ICsvPriceImportReporter
    {
        private readonly Configuration _configuration;
        private readonly StreamWriter _streamWriter;
        private const string ErrorsColumnName = "Error description";

        public CsvPriceImportReporter(Stream stream, Configuration configuration)
        {
            _streamWriter = new StreamWriter(stream);
            _configuration = configuration;
        }

        public async Task WriteAsync(ImportError error)
        {
            await _streamWriter.WriteLineAsync(GetLine(error));
        }

        public void Write(ImportError error)
        {
            _streamWriter.WriteLine(GetLine(error));
        }

        public void Dispose()
        {
            _streamWriter.Dispose();
        }


        private string GetLine(ImportError importError)
        {
            var result = $"{importError.Error}{_configuration.Delimiter}{importError.RawRow.TrimEnd()}";

            return result;
        }

        public void WriteHeader(string header)
        {
            _streamWriter.WriteLine($"{ErrorsColumnName}{_configuration.Delimiter}{header}");
        }
    }
}
