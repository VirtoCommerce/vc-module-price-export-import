using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.SimpleExportImportModule.Core.Models
{
    public sealed class ImportProgressInfo: ValueObject
    {
        public ImportProgressInfo()
        {
            Errors = new List<string>();
        }

        public string Description { get; set; }

        public int ProcessedCount { get; set; }

        public int TotalCount { get; set; }

        public int ErrorCount { get; set; }

        public ICollection<string> Errors { get; set; }
    }
}
