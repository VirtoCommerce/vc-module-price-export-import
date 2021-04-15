using System;
using System.Linq;
using CsvHelper.Configuration.Attributes;
using VirtoCommerce.SimpleExportImportModule.Data.Models;

namespace VirtoCommerce.SimpleExportImportModule.Data.Services
{
    public static class CsvPriceImportHelper
    {
        public static string[] GetImportPriceRequiredColumns()
        {
            var requiredColumns = typeof(CsvPrice).GetProperties()
                .Select(p =>
                    ((NameAttribute)Attribute.GetCustomAttribute(p, typeof(NameAttribute)))?.Names.First() ??
                    p.Name).ToArray();

            return requiredColumns;
        }

        public static string[] GetImportPriceRequiredValueColumns()
        {
            var requiredColumns = typeof(CsvPrice).GetProperties().Where(x => Nullable.GetUnderlyingType(x.PropertyType) == null)
                .Select(p =>
                    ((NameAttribute)Attribute.GetCustomAttribute(p, typeof(NameAttribute)))?.Names.First() ??
                    p.Name).ToArray();

            return requiredColumns;
        }
    }
}
