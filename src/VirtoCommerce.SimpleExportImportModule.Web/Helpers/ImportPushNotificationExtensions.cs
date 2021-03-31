using VirtoCommerce.SimpleExportImportModule.Core.Models;

namespace VirtoCommerce.SimpleExportImportModule.Web.Helpers
{
    public static class ImportPushNotificationExtensions
    {
            public static void Patch(this ImportPushNotification target, ImportProgressInfo source)
            {
                target.Description = source.Description;
                target.ProcessedCount = source.ProcessedCount;
                target.TotalCount = source.TotalCount;
                target.ErrorCount = source.ErrorCount;
                target.Errors = source.Errors;
            }
        
    }
}
