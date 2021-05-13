namespace VirtoCommerce.PriceExportImportModule.Core.Models
{
    public sealed class ImportDataRequest
    {
        public string PricelistId { get; set; }

        public ImportMode ImportMode { get; set; }

        public string FilePath { get; set; }
    }
}
