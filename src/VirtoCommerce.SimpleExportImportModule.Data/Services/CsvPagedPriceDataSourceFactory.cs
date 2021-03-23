using System.Globalization;
using System.IO;
using CsvHelper.Configuration;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.SimpleExportImportModule.Core.Services;

namespace VirtoCommerce.SimpleExportImportModule.Data.Services
{
    public sealed class CsvPagedPriceDataSourceFactory: ICsvPagedPriceDataSourceFactory
    {
        private readonly IProductSearchService _productSearchService;

        public CsvPagedPriceDataSourceFactory(IProductSearchService productSearchService)
        {
            _productSearchService = productSearchService;
        }

        public ICsvPagedPriceDataSource Create(Stream file, int pageSize)
        {
            return new CsvPagedPriceDataSource(_productSearchService, file, pageSize,
                new Configuration(CultureInfo.InvariantCulture) { Delimiter = ";" });
        }
    }
}