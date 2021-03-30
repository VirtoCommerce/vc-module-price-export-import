using System;

namespace VirtoCommerce.SimpleExportImportModule.Core.Models
{
    public class ImportDataValidationResult
    {
        public ImportDataValidationResult()
        {
            Errors = Array.Empty<string>();
        }
        public string[] Errors { get; set; }
    }
}
