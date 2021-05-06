using System.Threading.Tasks;

namespace VirtoCommerce.PriceExportImportModule.Core.Services
{
    public interface ICsvPriceImportReporterFactory
    {
        Task<ICsvPriceImportReporter> CreateAsync(string reportFilePath, string delimiter);
    }
}
