using System;

namespace VirtoCommerce.PriceExportImportModule.Core.Models
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
