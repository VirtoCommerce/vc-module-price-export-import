namespace VirtoCommerce.SimpleExportImportModule.Core.Models
{
    public sealed class ImportValidationState
    {
        public ImportProductPrice InvalidImportProductPrice { get; set; }

        public string FieldName { get; set; }
    }
}
