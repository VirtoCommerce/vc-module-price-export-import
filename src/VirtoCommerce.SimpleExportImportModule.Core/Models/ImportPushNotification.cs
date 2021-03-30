using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using VirtoCommerce.Platform.Core.PushNotifications;

namespace VirtoCommerce.SimpleExportImportModule.Core.Models
{
    public sealed class ImportPushNotification: PushNotification
    {
        public ImportPushNotification(string creator)
            : base(creator)
        {
            Errors = new List<string>();
        }

        [JsonProperty("jobId")]
        public string JobId { get; set; }

        [JsonProperty("finished")]
        public DateTime Finished { get; set; }

        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }

        [JsonProperty("processedCount")]
        public int ProcessedCount { get; set; }

        [JsonProperty("errorCount")]
        public int ErrorCount { get; set; }

        [JsonProperty("errors")]
        public ICollection<string> Errors { get; set; }

        public string ReportUrl { get; set; }
    }
}
