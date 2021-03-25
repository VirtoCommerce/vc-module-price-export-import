using CsvHelper.Configuration.Attributes;

namespace VirtoCommerce.SimpleExportImportModule.Data.Models
{
    public sealed class CsvPrice
    {
        [Name("SKU")]
        public string Sku { get; set; }
        
        [Name("Min quantity")]
        public int MinQuantity { get; set; }
        
        [Name("List price")]
        public decimal ListPrice { get; set; }
        
        [Name("Sale price")]
        public decimal? SalePrice { get; set; }
    }
}
