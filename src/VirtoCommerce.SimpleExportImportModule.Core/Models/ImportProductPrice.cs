using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Model;

namespace VirtoCommerce.SimpleExportImportModule.Core.Models
{
    public class ImportProductPrice
    {
        public string ProductId { get; set; }

        public string ProductCode { get; set; }

        public CatalogProduct Product { get; set; }

        public Price Price { get; set; }
    }
}