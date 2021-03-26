using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
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
        private readonly StreamReader _streamReader;
        private readonly CsvReader _csvReader;
        private int? _totalCount;

        public CsvPagedPriceDataSource(IProductSearchService productSearchService, Stream stream, int pageSize, Configuration configuration)
        {
            _productSearchService = productSearchService;
            _stream = stream;
            _streamReader = new StreamReader(stream);
            configuration.ReadingExceptionOccurred = ex => false;
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

            var totalCount = 0;

            try
            {
                _csvReader.Read();
                _csvReader.ReadHeader();
                _csvReader.ValidateHeader<CsvPrice>();
            }
            catch (ValidationException)
            {
                totalCount++;
            }

            while (_csvReader.Read())
            {
                totalCount++;
            }

            _totalCount = totalCount;

            _stream.Position = 0;

            return _totalCount.Value;
        }

        public async Task FetchAsync()
        {
            var records = _csvReader.GetRecords<CsvPrice>().Skip(CurrentPageNumber * PageSize).Take(PageSize).ToArray();
            CurrentPageNumber++;

            var skus = records.Select(x => x.Sku).ToArray();
            var products = await _productSearchService.SearchProductsAsync(new ProductSearchCriteria { Skus = skus });

            Items = records.Select(record =>
            {
                var product = products.Results.FirstOrDefault(p => p.Code == record.Sku);

                return new ImportProductPrice
                {
                    ProductId = product?.Id,
                    ProductCode = record.Sku,
                    Product = product,
                    Price = new Price
                    {
                        ProductId = product?.Id,
                        MinQuantity = record.MinQuantity,
                        List = record.ListPrice,
                        Sale = record.SalePrice
                    }

                };
            }).ToArray();
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
