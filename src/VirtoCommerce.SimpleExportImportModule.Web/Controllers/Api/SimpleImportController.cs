using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SimpleExportImportModule.Core;
using VirtoCommerce.SimpleExportImportModule.Core.Models;
using VirtoCommerce.SimpleExportImportModule.Core.Services;

namespace VirtoCommerce.SimpleExportImportModule.Web.Controllers.Api
{
    [Route("api/pricing/import")]
    [Authorize(ModuleConstants.Security.Permissions.ImportAccess)]
    [ApiController]
    public class SimpleImportController : ControllerBase
    {
        private readonly IBlobStorageProvider _blobStorageProvider;
        private readonly ICsvPagedPriceDataSourceFactory _csvPagedPriceDataSourceFactory;

        public SimpleImportController(IBlobStorageProvider blobStorageProvider, ICsvPagedPriceDataSourceFactory csvPagedPriceDataSourceFactory)
        {
            _blobStorageProvider = blobStorageProvider;
            _csvPagedPriceDataSourceFactory = csvPagedPriceDataSourceFactory;
        }

        [HttpPost]
        [Route("preview")]
        public async Task<ActionResult<ImportDataPreview>> GetImportPreview([FromBody] ImportDataPreviewRequest request)
        {
            if (request.FileUrl.IsNullOrEmpty())
            {
                return BadRequest($"{nameof(request.FileUrl)} can not be null");
            }

            var blobInfo = await _blobStorageProvider.GetBlobInfoAsync(request.FileUrl);

            if (blobInfo == null)
            {
                return BadRequest("Blob with the such url does not exist.");
            }

            var blobStream = _blobStorageProvider.OpenRead(request.FileUrl);
            using var csvDataSource = _csvPagedPriceDataSourceFactory.Create(blobStream, 10);

            var result = new ImportDataPreview
            {
                TotalCount = csvDataSource.GetTotalCount()
            };

            await csvDataSource.FetchAsync();

            result.Results = csvDataSource.Items;

            return Ok(result);
        }
    }
}
