using System;
using System.Threading.Tasks;
using VirtoCommerce.SimpleExportImportModule.Core.Models;

namespace VirtoCommerce.SimpleExportImportModule.Core.Services
{
    public interface ICsvPriceImportReporter : IDisposable
    {
        Task WriteAsync(ImportError error); 
    }
}
