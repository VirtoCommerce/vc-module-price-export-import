using System.IO;
using CsvHelper.Configuration;

namespace VirtoCommerce.SimpleExportImportModule.Core.Services
{
    public interface ICsvPagedPriceDataSourceFactory
    {
        ICsvPagedPriceDataSource Create(Stream stream, int pageSize, Configuration configuration = null);
    }
}
