using CsvHelper.Configuration;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.SimpleExportImportModule.Core.Models;
using VirtoCommerce.SimpleExportImportModule.Core.Services;

namespace VirtoCommerce.SimpleExportImportModule.Data.Services
{
    public sealed class CsvPagedPriceDataSourceFactory : ICsvPagedPriceDataSourceFactory
    {
        private readonly IBlobStorageProvider _blobStorageProvider;

        private readonly IProductSearchService _productSearchService;

        public CsvPagedPriceDataSourceFactory(IBlobStorageProvider blobStorageProvider, IProductSearchService productSearchService)
        {
            _blobStorageProvider = blobStorageProvider;
            _productSearchService = productSearchService;
        }

        public ICsvPagedPriceDataSource Create(string filePath, int pageSize, Configuration configuration = null)
        {
            return new CsvPagedPriceDataSource(filePath, _productSearchService, _blobStorageProvider, pageSize, configuration ?? new ImportConfiguration());
        }
    }
}
