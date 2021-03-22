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
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //        modelBuilder.Entity<MyModuleEntity>().ToTable("MyModule").HasKey(x => x.Id);
            //        modelBuilder.Entity<MyModuleEntity>().Property(x => x.Id).HasMaxLength(128);
            //        base.OnModelCreating(modelBuilder);
        }
    }
}

