using System;
using System.IO;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SimpleExportImportModule.Core.Models;

namespace VirtoCommerce.SimpleExportImportModule.Core.Services
{
    public interface ICsvPagedPriceDataImporter
    {
        Task ImportAsync(Stream stream, ImportDataRequest request, Action<ImportProgressInfo> progressCallback, ICancellationToken cancellationToken);
    }
}
