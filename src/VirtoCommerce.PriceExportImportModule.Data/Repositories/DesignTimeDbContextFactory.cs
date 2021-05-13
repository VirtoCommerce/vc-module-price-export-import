using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace VirtoCommerce.PriceExportImportModule.Data.Repositories
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<VirtoCommercePriceExportImportModuleDbContext>
    {
        public VirtoCommercePriceExportImportModuleDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<VirtoCommercePriceExportImportModuleDbContext>();

            builder.UseSqlServer("Data Source=(local);Initial Catalog=VirtoCommerce3;Persist Security Info=True;User ID=virto;Password=virto;MultipleActiveResultSets=True;Connect Timeout=30");

            return new VirtoCommercePriceExportImportModuleDbContext(builder.Options);
        }
    }
}
