using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.PushNotifications;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.PriceExportImportModule.Core;
using VirtoCommerce.PriceExportImportModule.Core.Models;
using VirtoCommerce.PriceExportImportModule.Core.Services;
using VirtoCommerce.PriceExportImportModule.Web.BackgroundJobs;

namespace VirtoCommerce.PriceExportImportModule.Web.Controllers.Api
{
    [Route("api/pricing/import")]
    [Authorize(ModuleConstants.Security.Permissions.ImportAccess)]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly IUserNameResolver _userNameResolver;
        private readonly IBlobStorageProvider _blobStorageProvider;
        private readonly IPushNotificationManager _pushNotificationManager;
        private readonly ICsvPagedPriceDataSourceFactory _csvPagedPriceDataSourceFactory;
        private readonly ICsvPriceDataValidator _csvPriceDataValidator;

        public ImportController(IUserNameResolver userNameResolver, IBlobStorageProvider blobStorageProvider, ICsvPriceDataValidator csvPriceDataValidator,
            IPushNotificationManager pushNotificationManager, ICsvPagedPriceDataSourceFactory csvPagedPriceDataSourceFactory)
        {
            _userNameResolver = userNameResolver;
            _blobStorageProvider = blobStorageProvider;
            _pushNotificationManager = pushNotificationManager;
            _csvPagedPriceDataSourceFactory = csvPagedPriceDataSourceFactory;
            _csvPriceDataValidator = csvPriceDataValidator;
        }

        [HttpPost]
        [Route("run")]
        public async Task<ActionResult<ImportPushNotification>> RunImport([FromBody] ImportDataRequest request)
        {
            var notification = new ImportPushNotification(_userNameResolver.GetCurrentUserName())
            {
                Title = "Prices import",
                Description = "Starting import task..."
            };
            await _pushNotificationManager.SendAsync(notification);

            notification.JobId = BackgroundJob.Enqueue<ImportJob>(importJob => importJob.ImportBackgroundAsync(request, notification, JobCancellationToken.Null, null));

            return Ok(notification);
        }

        [HttpPost]
        [Route("cancel")]
        public ActionResult CancelExport([FromBody] ImportCancellationRequest cancellationRequest)
        {
            BackgroundJob.Delete(cancellationRequest.JobId);
            return Ok();
        }

        [HttpPost]
        [Route("preview")]
        public async Task<ActionResult<ImportDataPreview>> GetImportPreview([FromBody] ImportDataPreviewRequest request)
        {
            if (request.FilePath.IsNullOrEmpty())
            {
                return BadRequest($"{nameof(request.FilePath)} can not be null");
            }

            var blobInfo = await _blobStorageProvider.GetBlobInfoAsync(request.FilePath);

            if (blobInfo == null)
            {
                return BadRequest("Blob with the such url does not exist.");
            }

            using var csvDataSource = _csvPagedPriceDataSourceFactory.Create(request.FilePath, 10);

            var result = new ImportDataPreview
            {
                TotalCount = csvDataSource.GetTotalCount()
            };

            await csvDataSource.FetchAsync();

            result.Results = csvDataSource.Items;

            return Ok(result);
        }

        [HttpPost]
        [Route("validate")]
        public async Task<ActionResult<ImportDataValidationResult>> Validate([FromBody] ImportDataValidationRequest request)
        {
            if (request.FilePath.IsNullOrEmpty())
            {
                return BadRequest($"{nameof(request.FilePath)} can not be null or empty.");
            }

            var result = await _csvPriceDataValidator.ValidateAsync(request.FilePath);

            return Ok(result);
        }
    }
}
