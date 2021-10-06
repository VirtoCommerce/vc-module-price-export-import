using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PriceExportImportModule.Core.Models;
using VirtoCommerce.PriceExportImportModule.Core.Services;
using VirtoCommerce.PriceExportImportModule.Data.Models;
using VirtoCommerce.PricingModule.Core.Model;

namespace VirtoCommerce.PriceExportImportModule.Data.Services
{
    public sealed class CsvPagedPriceDataSource : ICsvPagedPriceDataSource
    {
        private readonly IProductSearchService _productSearchService;
        private readonly Stream _stream;
        private readonly CsvConfiguration _configuration;
        private readonly StreamReader _streamReader;
        private readonly IReader _csvReader;
        private int? _totalCount;

        public CsvPagedPriceDataSource(
            string filePath,
            IProductSearchService productSearchService,
            IBlobStorageProvider blobStorageProvider,
            int pageSize,
            CsvConfiguration configuration,
            Func<TextReader, CsvConfiguration, IReader> csvReaderFactory)
        {
            _productSearchService = productSearchService;

            var stream = blobStorageProvider.OpenRead(filePath);
            _stream = stream;
            _streamReader = new StreamReader(stream);

            _configuration = configuration;
            _csvReader = csvReaderFactory(_streamReader, _configuration);

            PageSize = pageSize;
        }

        public int CurrentPageNumber { get; private set; }

        public int PageSize { get; }

        public string GetHeaderRaw()
        {
            var result = string.Empty;

            var streamPosition = _stream.Position;
            _stream.Seek(0, SeekOrigin.Begin);

            using var streamReader = new StreamReader(_stream, leaveOpen: true);
            using var csvReader = new CsvReader(streamReader, _configuration);

            try
            {
                csvReader.Read();
                csvReader.ReadHeader();
                csvReader.ValidateHeader<CsvPrice>();

                result = string.Join(csvReader.Configuration.Delimiter, csvReader.Context.Reader.HeaderRecord);

            }
            finally
            {
                _stream.Seek(streamPosition, SeekOrigin.Begin);
            }

            return result;
        }

        public int GetTotalCount()
        {
            if (_totalCount != null)
            {
                return _totalCount.Value;
            }

            _totalCount = 0;

            var streamPosition = _stream.Position;
            _stream.Seek(0, SeekOrigin.Begin);

            // Because of these properties are delegates we have to null them to fix false positive firing
            var originReadingExceptionOccurredDelegate = _configuration.ReadingExceptionOccurred;
            var originBadDataFoundDelegate = _configuration.BadDataFound;

            _configuration.ReadingExceptionOccurred = args => false;
            _configuration.BadDataFound = null;

            using var streamReader = new StreamReader(_stream, leaveOpen: true);
            using var csvReader = new CsvReader(streamReader, _configuration);
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

            // And after counting totals return back delegates
            _configuration.ReadingExceptionOccurred = originReadingExceptionOccurredDelegate;
            _configuration.BadDataFound = originBadDataFoundDelegate;

            return _totalCount.Value;
        }

        public async Task<bool> FetchAsync()
        {
            if (CurrentPageNumber * PageSize >= GetTotalCount())
            {
                Items = Array.Empty<ImportProductPrice>();
                return false;
            }

            var recordTuples = new List<(CsvPrice, string, int)>();

            for (var i = 0; i < PageSize && await _csvReader.ReadAsync(); i++)
            {
                if (_csvReader is VcCsvReader vcCsvReader)
                {
                    if (!vcCsvReader.IsFieldBadData)
                    {
                        var csvRecord = _csvReader.GetRecord<CsvPrice>();

                        var rawRecord = _csvReader.Context.Parser.RawRecord;
                        var row = _csvReader.Context.Parser.Row;

                        var recordTuple = (csvRecord, rawRecord, row);
                        recordTuples.Add(recordTuple);
                    }

                    vcCsvReader.IsFieldBadData = false;
                }
            }

            var skus = recordTuples.Where(x => x.Item1 != null).Select(x => x.Item1.Sku).ToArray();
            var products = await _productSearchService.SearchProductsAsync(new ProductSearchCriteria { Skus = skus, SearchInVariations = true, Take = skus.Length });

            Items = recordTuples.Where(x => x.Item1 != null).Select(record =>
              {
                  var product = products.Results.FirstOrDefault(p => p.Code == record.Item1.Sku);

                  var importProductPrice = new ImportProductPrice
                  {
                      ProductId = product?.Id,
                      Sku = record.Item1.Sku,
                      Product = product,
                      Price = AbstractTypeFactory<Price>.TryCreateInstance()
                  };
                  importProductPrice.Price.ProductId = product?.Id;
                  importProductPrice.Price.MinQuantity = record.Item1.MinQuantity;
                  importProductPrice.Price.List = record.Item1.ListPrice;
                  importProductPrice.Price.Sale = record.Item1.SalePrice;
                  importProductPrice.RawRecord = record.Item2;
                  importProductPrice.Row = record.Item3;

                  return importProductPrice;
              }).ToArray();

            CurrentPageNumber++;

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
