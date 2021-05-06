using System;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PriceExportImportModule.Core.Models;

namespace VirtoCommerce.PriceExportImportModule.Core.Services
{
    public interface ICsvPagedPriceDataImporter
    {
        Task ImportAsync(ImportDataRequest request, Action<ImportProgressInfo> progressCallback, ICancellationToken cancellationToken);
    }
}
