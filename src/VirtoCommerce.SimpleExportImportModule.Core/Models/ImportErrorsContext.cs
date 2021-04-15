using System.Collections.Generic;

namespace VirtoCommerce.SimpleExportImportModule.Core.Models
{
    public sealed class ImportErrorsContext
    {
        public IList<int> ErrorsRows { get; set; } = new List<int>();
    }
}
