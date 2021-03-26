using System;
using System.Threading.Tasks;
using VirtoCommerce.PricingModule.Core.Model;

namespace VirtoCommerce.SimpleExportImportModule.Core.Services
{
    public interface ICsvPagedPriceDataSource: IDisposable
    {
        int CurrentPageNumber { get; }

        int PageSize { get; }

        int GetTotalCount();

        Task FetchAsync();

        ProductPrice[] Items { get; }
    }
}
