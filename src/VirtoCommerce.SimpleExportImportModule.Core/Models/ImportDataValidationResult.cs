using System;

namespace VirtoCommerce.SimpleExportImportModule.Core.Models
{
    public sealed class ImportDataValidationResult
    {
        public ImportDataValidationResult()
        {
            Errors = Array.Empty<ImportDataValidationError>();
        }

        public ImportDataValidationError[] Errors { get; set; }
    }
}
