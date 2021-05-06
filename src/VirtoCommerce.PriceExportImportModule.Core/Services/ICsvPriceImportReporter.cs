using System;
using System.Threading.Tasks;
using VirtoCommerce.PriceExportImportModule.Core.Models;

namespace VirtoCommerce.PriceExportImportModule.Core.Services
{
    public interface ICsvPriceImportReporter : IAsyncDisposable
    {
        bool ReportIsNotEmpty { get; }
        void WriteHeader(string header);
        Task WriteAsync(ImportError error);
        void Write(ImportError error);
    }
}
