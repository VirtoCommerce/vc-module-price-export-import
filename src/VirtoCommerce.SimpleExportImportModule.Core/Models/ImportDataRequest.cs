namespace VirtoCommerce.SimpleExportImportModule.Core.Models
{
    public sealed class ImportDataRequest
    {
        public string PricelistId { get; set; }

        public ImportMode ImportMode { get; set; }

        public string FileUrl { get; set; }
    }
}
