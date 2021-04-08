using System.Collections.Generic;

namespace VirtoCommerce.SimpleExportImportModule.Core.Models
{
    public sealed class ImportErrorsContext
    {
        public IList<int> MissedColumnsRows { get; set; } = new List<int>();
    }
}
