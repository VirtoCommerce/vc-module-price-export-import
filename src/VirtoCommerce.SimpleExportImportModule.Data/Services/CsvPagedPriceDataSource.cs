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

        public CsvPagedPriceDataSource(string filePath, IProductSearchService productSearchService, IBlobStorageProvider blobStorageProvider, int pageSize, Configuration configuration)
        {
            _productSearchService = productSearchService;

            var stream = blobStorageProvider.OpenRead(filePath);

            _stream = stream;
            _streamReader = new StreamReader(stream);

            _configuration = configuration;
            _csvReader = new CsvReader(_streamReader, configuration);

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
            using var csvReader = new CsvReader(streamReader, _configuration, true);

            try
            {
                csvReader.Read();
                csvReader.ReadHeader();
                csvReader.ValidateHeader<CsvPrice>();

                result = string.Join(csvReader.Configuration.Delimiter, csvReader.Context.HeaderRecord);

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

            var recordTuples = new List<(CsvPrice, string, int)>();

            for (var i = 0; i < PageSize && await _csvReader.ReadAsync(); i++)
            {
                var csvRecord = _csvReader.GetRecord<CsvPrice>();

                var rawRecord = _csvReader.Context.RawRecord;
                var row = _csvReader.Context.Row;

                var recordTuple = (csvRecord, rawRecord, row);
                recordTuples.Add(recordTuple);

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
