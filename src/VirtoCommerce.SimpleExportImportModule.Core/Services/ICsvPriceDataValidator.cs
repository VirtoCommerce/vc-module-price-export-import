using System.Threading.Tasks;
using VirtoCommerce.PriceExportImportModule.Core.Models;

namespace VirtoCommerce.PriceExportImportModule.Core.Services
{
    public interface ICsvPriceDataValidator
    {
        Task<ImportDataValidationResult> ValidateAsync(string filePath);
    }
}
