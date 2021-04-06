using System;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using VirtoCommerce.SimpleExportImportModule.Core.Models;
using VirtoCommerce.SimpleExportImportModule.Core.Services;

namespace VirtoCommerce.SimpleExportImportModule.Data.Services
{
    public sealed class CsvPriceImportReporter : ICsvPriceImportReporter
    {
        private readonly Stream _stream;
        private readonly Configuration _configuration;
        private readonly StreamWriter _streamWriter;

        public CsvPriceImportReporter(Stream stream, Configuration configuration)
        {
            _stream = stream;
            _streamWriter = new StreamWriter(stream);
            _configuration = configuration;
        }

        public async Task WriteAsync(ImportError importError)
        {
            var line = $"{importError.Error}{_configuration.Delimiter}{importError.RawRow}";
            await _streamWriter.WriteLineAsync(line);
        }

        public void Dispose()
        {
            _streamWriter.Dispose();
            _stream.Dispose();
        }
    }
}
