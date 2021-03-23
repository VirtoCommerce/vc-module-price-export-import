using EntityFrameworkCore.Triggers;
using Microsoft.EntityFrameworkCore;

namespace VirtoCommerce.SimpleExportImportModule.Data.Repositories
{
    public class VirtoCommerceSimpleExportImportModuleDbContext : DbContextWithTriggers
    {
        public VirtoCommerceSimpleExportImportModuleDbContext(DbContextOptions<VirtoCommerceSimpleExportImportModuleDbContext> options)
          : base(options)
        {
        }

        protected VirtoCommerceSimpleExportImportModuleDbContext(DbContextOptions options)
            : base(options)
        {
        }
    }
}

