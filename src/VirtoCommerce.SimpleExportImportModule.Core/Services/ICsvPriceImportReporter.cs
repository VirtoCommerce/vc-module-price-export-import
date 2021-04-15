using System;
using System.Threading.Tasks;
using VirtoCommerce.SimpleExportImportModule.Core.Models;

namespace VirtoCommerce.SimpleExportImportModule.Core.Services
{
    public interface ICsvPriceImportReporter : IDisposable
    {
        void WriteHeader(string header);
        Task WriteAsync(ImportError error);
        void Write(ImportError error);
    }
}
