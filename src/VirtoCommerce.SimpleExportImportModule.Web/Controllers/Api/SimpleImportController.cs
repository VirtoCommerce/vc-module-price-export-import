using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.PushNotifications;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.SimpleExportImportModule.Core;
using VirtoCommerce.SimpleExportImportModule.Core.Models;
using VirtoCommerce.SimpleExportImportModule.Core.Services;
using VirtoCommerce.SimpleExportImportModule.Web.BackgroundJobs;

namespace VirtoCommerce.SimpleExportImportModule.Web.Controllers.Api
{
    [Route("api/pricing/import")]
    [Authorize(ModuleConstants.Security.Permissions.ImportAccess)]
    [ApiController]
    public class SimpleImportController : ControllerBase
    {
        private readonly IUserNameResolver _userNameResolver;
        private readonly IBlobStorageProvider _blobStorageProvider;
        private readonly IPushNotificationManager _pushNotificationManager;
        private readonly ICsvPagedPriceDataSourceFactory _csvPagedPriceDataSourceFactory;

        public SimpleImportController(IUserNameResolver userNameResolver, IBlobStorageProvider blobStorageProvider,
            IPushNotificationManager pushNotificationManager, ICsvPagedPriceDataSourceFactory csvPagedPriceDataSourceFactory)
        {
            _userNameResolver = userNameResolver;
            _blobStorageProvider = blobStorageProvider;
            _pushNotificationManager = pushNotificationManager;
            _csvPagedPriceDataSourceFactory = csvPagedPriceDataSourceFactory;
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
            if (request.FileUrl.IsNullOrEmpty())
            {
                return BadRequest($"{nameof(request.FileUrl)} can not be null");
            }

            var blobInfo = await _blobStorageProvider.GetBlobInfoAsync(request.FileUrl);

            if (blobInfo == null)
            {
                return BadRequest("Blob with the such url does not exist.");
            }

            await using var blobStream = _blobStorageProvider.OpenRead(request.FileUrl);
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
