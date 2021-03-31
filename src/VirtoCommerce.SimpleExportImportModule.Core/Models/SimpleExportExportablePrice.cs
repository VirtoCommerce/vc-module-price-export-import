using System;
using VirtoCommerce.ExportModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Data.ExportImport;

namespace VirtoCommerce.SimpleExportImportModule.Core.Models
{
    public class SimpleExportExportablePrice : ExportablePrice
    {
        public override ExportablePrice FromModel(Price source)
        {
            base.FromModel(source);

            CreatedDate = source.CreatedDate;
            ModifiedDate = source.ModifiedDate;
            StartDate = source.StartDate;
            EndDate = source.EndDate;
            ModifiedBy = source.ModifiedBy;
            CreatedBy = source.CreatedBy;

            return this;

        }

        public override IExportable ToTabular()
        {
            var result = (SimpleExportTabularPrice)base.ToTabular();

            result.Code = Code;
            result.ModifiedDate = ModifiedDate;
            result.StartDate = StartDate;
            result.EndDate = EndDate;
            result.CreatedDate = CreatedDate;
            result.CreatedBy = CreatedBy;
            result.ModifiedBy = ModifiedBy;

            return result;
        }
    }
}
