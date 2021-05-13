namespace VirtoCommerce.PriceExportImportModule.Core.Models
{
    public sealed class ImportError
    {
        public string Error { get; set; }

        public string RawRow { get; set; }
    }
}
