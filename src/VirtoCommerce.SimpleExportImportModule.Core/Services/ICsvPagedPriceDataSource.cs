using System;
using System.Threading.Tasks;
using VirtoCommerce.SimpleExportImportModule.Core.Models;

namespace VirtoCommerce.SimpleExportImportModule.Core.Services
{
    public interface ICsvPagedPriceDataSource : IDisposable
    {
        int CurrentPageNumber { get; }

        int PageSize { get; }

        int GetTotalCount();

        Task FetchAsync();

        ImportProductPrice[] Items { get; }
    }
}
