using System.IO;
using System.Threading.Tasks;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PriceExportImportModule.Core.Models;
using VirtoCommerce.PriceExportImportModule.Core.Services;

namespace VirtoCommerce.PriceExportImportModule.Data.Services
{
    public sealed class CsvPriceImportReporter : ICsvPriceImportReporter
    {
        private readonly IBlobStorageProvider _blobStorageProvider;
        private readonly string _reportFilePath;
        private readonly string _delimiter;
        private readonly StreamWriter _streamWriter;
        private const string ErrorsColumnName = "Error description";
        private readonly object _lock = new object();

        public bool ReportIsNotEmpty { get; private set; } = false;

        public CsvPriceImportReporter(string reportFilePath, IBlobStorageProvider blobStorageProvider, string delimiter)
        {
            _reportFilePath = reportFilePath;
            _delimiter = delimiter;
            _blobStorageProvider = blobStorageProvider;
            var stream = _blobStorageProvider.OpenWrite(reportFilePath);
            _streamWriter = new StreamWriter(stream);
        }

        public async Task WriteAsync(ImportError error)
        {
            using (await AsyncLock.GetLockByKey(_reportFilePath).GetReleaserAsync())
            {
                ReportIsNotEmpty = true;
                await _streamWriter.WriteLineAsync(GetLine(error));
            }
        }

        public void Write(ImportError error)
        {
            lock (_lock)
            {
                ReportIsNotEmpty = true;
                _streamWriter.WriteLine(GetLine(error));
            }
        }

        public void WriteHeader(string header)
        {
            lock (_lock)
            {
                _streamWriter.WriteLine($"{ErrorsColumnName}{_delimiter}{header}");
            }
        }

        public async ValueTask DisposeAsync()
        {
            using (await AsyncLock.GetLockByKey(_reportFilePath).GetReleaserAsync())
            {
                await _streamWriter.DisposeAsync();

                if (!ReportIsNotEmpty)
                {
                    await _blobStorageProvider.RemoveAsync(new[] { _reportFilePath });
                }
            }
        }


        private string GetLine(ImportError importError)
        {
            var result = $"{importError.Error}{_delimiter}{importError.RawRow.TrimEnd()}";

            return result;
        }
    }
}
