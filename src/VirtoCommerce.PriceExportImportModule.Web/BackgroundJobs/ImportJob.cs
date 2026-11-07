using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Exceptions;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.Platform.Core.PushNotifications;
using VirtoCommerce.PriceExportImportModule.Core.Models;
using VirtoCommerce.PriceExportImportModule.Core.Services;
using VirtoCommerce.PriceExportImportModule.Web.Helpers;

namespace VirtoCommerce.PriceExportImportModule.Web.BackgroundJobs
{
    public class ImportJobPayload
    {
        public ImportDataRequest Request { get; set; }
        public ImportPushNotification Notification { get; set; }
    }

    /// <summary>
    /// Engine-agnostic price import job. Replaces the former Hangfire job that used <c>PerformContext</c> and
    /// <c>IJobCancellationToken</c>: the job id now comes from <see cref="IJobExecutionContext.JobId"/> and cancellation
    /// from the handler's <see cref="CancellationToken"/>.
    /// </summary>
    public sealed class ImportJob : IBackgroundJobHandler<ImportJobPayload>
    {
        private readonly ICsvPagedPriceDataImporter _dataImporter;
        private readonly IPushNotificationManager _pushNotificationManager;

        public ImportJob(ICsvPagedPriceDataImporter dataImporter,
            IPushNotificationManager pushNotificationManager)
        {
            _dataImporter = dataImporter;
            _pushNotificationManager = pushNotificationManager;
        }

        public async Task Execute(ImportJobPayload payload, IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            var request = payload.Request;
            var pushNotification = payload.Notification;

            ValidateParameters(pushNotification);

            try
            {
                await _dataImporter.ImportAsync(request,
                    progressInfo => ProgressCallback(progressInfo, pushNotification, context),
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // job is aborted, do nothing
            }

            catch (Exception ex)
            {
                pushNotification.Errors.Add(ex.ExpandExceptionMessage());
                pushNotification.ErrorCount++;
            }

            finally
            {
                pushNotification.Description = "Import finished";
                pushNotification.Finished = DateTime.UtcNow;

                await _pushNotificationManager.SendAsync(pushNotification);
            }
        }

        private void ProgressCallback(ImportProgressInfo x, ImportPushNotification pushNotification, IJobExecutionContext context)
        {
            pushNotification.Patch(x);
            pushNotification.JobId = context.JobId;
            _pushNotificationManager.Send(pushNotification);
        }

        private static void ValidateParameters(ImportPushNotification pushNotification)
        {
            if (pushNotification == null)
            {
                throw new ArgumentNullException(nameof(pushNotification));
            }
        }
    }
}
