using System;
using VirtoCommerce.PricingModule.Data.ExportImport;

namespace VirtoCommerce.PriceExportImportModule.Core.Models
{
    public sealed class ExportTabularPrice : TabularPrice
    {
        public string Code { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
    }
}
