using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.SimpleExportImportModule.Core.Services;

namespace VirtoCommerce.SimpleExportImportModule.Data.Services
{
    public sealed class CsvPriceImportReporterFactory : ICsvPriceImportReporterFactory
    {
        private readonly IBlobStorageProvider _blobStorageProvider;
        public CsvPriceImportReporterFactory(IBlobStorageProvider blobStorageProvider)
        {
            _blobStorageProvider = blobStorageProvider;
        }

        public async Task<ICsvPriceImportReporter> CreateAsync(string reportFilePath, string delimiter)
        {
            var reportBlob = await _blobStorageProvider.GetBlobInfoAsync(reportFilePath);

            if (reportBlob != null)
            {
                await _blobStorageProvider.RemoveAsync(new[] { reportFilePath });
            }

            return new CsvPriceImportReporter(reportFilePath, _blobStorageProvider, delimiter);
        }
    }
}
