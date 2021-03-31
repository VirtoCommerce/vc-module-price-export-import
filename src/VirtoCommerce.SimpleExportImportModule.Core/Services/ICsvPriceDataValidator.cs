using System.Threading.Tasks;
using VirtoCommerce.SimpleExportImportModule.Core.Models;

namespace VirtoCommerce.SimpleExportImportModule.Core.Services
{
    public interface ICsvPriceDataValidator
    {
        Task<ImportDataValidationResult> ValidateAsync(string fileUrl);
    }
}
