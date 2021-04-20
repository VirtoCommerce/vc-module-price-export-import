using System.Threading.Tasks;

namespace VirtoCommerce.SimpleExportImportModule.Core.Services
{
    public interface ICsvPriceImportReporterFactory
    {
        Task<ICsvPriceImportReporter> CreateAsync(string reportFilePath, string delimiter);
    }
}
