using CsvHelper.Configuration;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.PriceExportImportModule.Core.Models;
using VirtoCommerce.PriceExportImportModule.Core.Services;

namespace VirtoCommerce.PriceExportImportModule.Data.Services
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

        public ICsvPagedPriceDataSource Create(string filePath, int pageSize, CsvConfiguration configuration = null)
        {
            return new CsvPagedPriceDataSource(filePath, _productSearchService, _blobStorageProvider, pageSize, configuration ?? ImportConfiguration.GetCsvConfiguration());
        }
    }
}
