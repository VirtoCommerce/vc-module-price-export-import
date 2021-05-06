using System;
using System.Threading.Tasks;
using VirtoCommerce.PriceExportImportModule.Core.Models;

namespace VirtoCommerce.PriceExportImportModule.Core.Services
{
    public interface ICsvPagedPriceDataSource : IDisposable
    {
        int CurrentPageNumber { get; }

        int PageSize { get; }

        string GetHeaderRaw();

        int GetTotalCount();

        Task<bool> FetchAsync();

        ImportProductPrice[] Items { get; }
    }
}
