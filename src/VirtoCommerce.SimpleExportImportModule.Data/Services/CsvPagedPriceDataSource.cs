using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.SimpleExportImportModule.Core.Models;
using VirtoCommerce.SimpleExportImportModule.Core.Services;
using VirtoCommerce.SimpleExportImportModule.Data.Models;

namespace VirtoCommerce.SimpleExportImportModule.Data.Services
{
    public sealed class CsvPagedPriceDataSource : ICsvPagedPriceDataSource
    {
        private readonly IProductSearchService _productSearchService;
        private readonly Stream _stream;
        private readonly Configuration _configuration;
        private readonly StreamReader _streamReader;
        private readonly CsvReader _csvReader;
        private int? _totalCount;

        public CsvPagedPriceDataSource(IProductSearchService productSearchService, Stream stream, int pageSize, Configuration configuration)
        {
            _productSearchService = productSearchService;

            _stream = stream;
            _streamReader = new StreamReader(stream);

            _configuration = configuration;
            _csvReader = new CsvReader(_streamReader, configuration);

            PageSize = pageSize;
        }

        public int CurrentPageNumber { get; private set; }

        public int PageSize { get; }

        public int GetTotalCount()
        {
            if (_totalCount != null)
            {
                return _totalCount.Value;
            }

            _totalCount = 0;

            var streamPosition = _stream.Position;
            _stream.Seek(0, SeekOrigin.Begin);

            using var streamReader = new StreamReader(_stream, leaveOpen: true);
            using var csvReader = new CsvReader(streamReader, _configuration, true);
            try
            {
                csvReader.Read();
                csvReader.ReadHeader();
                csvReader.ValidateHeader<CsvPrice>();
            }
            catch (ValidationException)
            {
                _totalCount++;
            }

            while (csvReader.Read())
            {
                _totalCount++;
            }

            _stream.Seek(streamPosition, SeekOrigin.Begin);

            return _totalCount.Value;
        }

        public async Task<bool> FetchAsync()
        {
            if (CurrentPageNumber * PageSize >= GetTotalCount())
            {
                Items = Array.Empty<ImportProductPrice>();
                return false;
            }

            // CSV Reader can only move forward, i.e. Skip will not work: after reading N items we can read only next N items
            var records = _csvReader.GetRecords<CsvPrice>().Take(PageSize).ToArray();
            CurrentPageNumber++;

            var skus = records.Select(x => x.Sku).ToArray();
            var products = await _productSearchService.SearchProductsAsync(new ProductSearchCriteria { Skus = skus });

            Items = records.Select(record =>
            {
                var product = products.Results.FirstOrDefault(p => p.Code == record.Sku);

                var importProductPrice = new ImportProductPrice
                {
                    ProductId = product?.Id,
                    Sku = record.Sku,
                    Product = product,
                    Price = AbstractTypeFactory<Price>.TryCreateInstance()
                };
                importProductPrice.Price.ProductId = product?.Id;
                importProductPrice.Price.MinQuantity = record.MinQuantity;
                importProductPrice.Price.List = record.ListPrice;
                importProductPrice.Price.Sale = record.SalePrice;
                return importProductPrice;
            }).ToArray();

            return true;
        }

        public ImportProductPrice[] Items { get; private set; }

        public void Dispose()
        {
            _csvReader.Dispose();
            _streamReader.Dispose();
            _stream.Dispose();
        }
    }
}
