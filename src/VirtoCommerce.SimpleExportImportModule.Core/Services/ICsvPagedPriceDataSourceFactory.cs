using CsvHelper.Configuration;

namespace VirtoCommerce.SimpleExportImportModule.Core.Services
{
    public interface ICsvPagedPriceDataSourceFactory
    {
        ICsvPagedPriceDataSource Create(string filePath, int pageSize, Configuration configuration = null);
    }
}
