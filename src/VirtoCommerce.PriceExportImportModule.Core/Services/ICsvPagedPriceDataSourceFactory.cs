using CsvHelper.Configuration;

namespace VirtoCommerce.PriceExportImportModule.Core.Services
{
    public interface ICsvPagedPriceDataSourceFactory
    {
        ICsvPagedPriceDataSource Create(string filePath, int pageSize, CsvConfiguration configuration = null);
    }
}
