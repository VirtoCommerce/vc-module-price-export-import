using System.IO;

namespace VirtoCommerce.SimpleExportImportModule.Core.Services
{
    public interface ICsvPagedPriceDataSourceFactory
    {
        ICsvPagedPriceDataSource Create(Stream file, int pageSize);
    }
}
