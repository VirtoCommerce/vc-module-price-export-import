using EntityFrameworkCore.Triggers;
using Microsoft.EntityFrameworkCore;

namespace VirtoCommerce.PriceExportImportModule.Data.Repositories
{
    public class VirtoCommercePriceExportImportModuleDbContext : DbContextWithTriggers
    {
        public VirtoCommercePriceExportImportModuleDbContext(DbContextOptions<VirtoCommercePriceExportImportModuleDbContext> options)
          : base(options)
        {
        }

        protected VirtoCommercePriceExportImportModuleDbContext(DbContextOptions options)
            : base(options)
        {
        }
    }
}

