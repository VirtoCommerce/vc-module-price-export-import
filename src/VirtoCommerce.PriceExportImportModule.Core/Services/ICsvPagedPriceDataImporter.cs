using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PriceExportImportModule.Core.Models;

namespace VirtoCommerce.PriceExportImportModule.Core.Services
{
    public interface ICsvPagedPriceDataImporter
    {
        Task ImportAsync(ImportDataRequest request, Action<ImportProgressInfo> progressCallback, CancellationToken cancellationToken);

        [Obsolete("Use the CancellationToken overload.", DiagnosticId = "VC0015", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
        Task ImportAsync(ImportDataRequest request, Action<ImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
            => ImportAsync(request, progressCallback, (cancellationToken as CancellationTokenWrapper)?.CancellationToken ?? CancellationToken.None);
    }
}
