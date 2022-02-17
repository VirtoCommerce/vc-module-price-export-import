using System;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.PriceExportImportModule.Core.Models;
using VirtoCommerce.PriceExportImportModule.Core.Services;

namespace VirtoCommerce.PriceExportImportModule.Data.Services
{
    public sealed class CsvPagedPriceDataSourceFactory : ICsvPagedPriceDataSourceFactory
    {
        private readonly IBlobStorageProvider _blobStorageProvider;
        private readonly IProductSearchService _productSearchService;
        private readonly ImportConfigurationFactory _importConfigurationFactory;
        private readonly Func<TextReader, CsvConfiguration, IReader> _csvReaderFactory;

        public CsvPagedPriceDataSourceFactory(
            IBlobStorageProvider blobStorageProvider,
            IProductSearchService productSearchService,
            ImportConfigurationFactory importConfigurationFactory,
            Func<TextReader, CsvConfiguration, IReader> csvReaderFactory)
        {
            _blobStorageProvider = blobStorageProvider;
            _productSearchService = productSearchService;
            _importConfigurationFactory = importConfigurationFactory;
            _csvReaderFactory = csvReaderFactory;
        }

        public ICsvPagedPriceDataSource Create(string filePath, int pageSize, CsvConfiguration configuration = null)
        {
            return new CsvPagedPriceDataSource(filePath, _productSearchService, _blobStorageProvider, pageSize, configuration ?? _importConfigurationFactory.Create(), _csvReaderFactory);
        }
    }
}
