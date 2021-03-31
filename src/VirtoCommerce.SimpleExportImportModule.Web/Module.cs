using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.FeatureManagementModule.Core.Services;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.SimpleExportImportModule.Core;
using VirtoCommerce.SimpleExportImportModule.Core.Services;
using VirtoCommerce.SimpleExportImportModule.Data.Repositories;
using featureManagementCore = VirtoCommerce.FeatureManagementModule.Core;
using simpleExportImportCore = VirtoCommerce.SimpleExportImportModule.Core;
using VirtoCommerce.SimpleExportImportModule.Data.Services;
using VirtoCommerce.SimpleExportImportModule.Core.Models;
using VirtoCommerce.Platform.Core.Settings;
using Microsoft.Extensions.Options;
using VirtoCommerce.PricingModule.Data.ExportImport;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.SimpleExportImportModule.Web
{
    public class Module : IModule, IHasConfiguration
    {
        public ManifestModuleInfo ModuleInfo { get; set; }

        public IConfiguration Configuration { get; set; }

        public void Initialize(IServiceCollection serviceCollection)
        {
            // database initialization
            var configuration = serviceCollection.BuildServiceProvider().GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("VirtoCommerce.SimpleExportImport") ?? configuration.GetConnectionString("VirtoCommerce");
            serviceCollection.AddDbContext<VirtoCommerceSimpleExportImportModuleDbContext>(options => options.UseSqlServer(connectionString));

            serviceCollection.AddTransient<ICsvPagedPriceDataSourceFactory, CsvPagedPriceDataSourceFactory>();

            serviceCollection.AddOptions<SimpleExportOptions>().Bind(Configuration.GetSection("SimpleExportImport:SimpleExport")).ValidateDataAnnotations();
        }

        public void PostInitialize(IApplicationBuilder appBuilder)
        {
            // prices
            AbstractTypeFactory<TabularPrice>.OverrideType<TabularPrice, SimpleExportTabularPrice>();
            AbstractTypeFactory<ExportablePrice>.OverrideType<ExportablePrice, SimpleExportExportablePrice>();

            var settingsRegistrar = appBuilder.ApplicationServices.GetRequiredService<ISettingsRegistrar>();
            settingsRegistrar.RegisterSettings(ModuleConstants.Settings.General.AllSettings, ModuleInfo.Id);

            var settingsManager = appBuilder.ApplicationServices.GetService<ISettingsManager>();
            var simpleExportImportOptions = appBuilder.ApplicationServices.GetService<IOptions<SimpleExportOptions>>().Value;
            settingsManager.SetValue(ModuleConstants.Settings.General.SimpleExportLimitOfLines.Name,
                simpleExportImportOptions.LimitOfLines ?? ModuleConstants.Settings.General.SimpleExportLimitOfLines.DefaultValue);

            // register permissions
            var permissionsProvider = appBuilder.ApplicationServices.GetRequiredService<IPermissionsRegistrar>();
            permissionsProvider.RegisterPermissions(ModuleConstants.Security.Permissions.AllPermissions.Select(x =>
                new Permission()
                {
                    GroupName = "SimpleExportImport",
                    ModuleId = ModuleInfo.Id,
                    Name = x
                }).ToArray());

            var featureStorage = appBuilder.ApplicationServices.GetService<IFeatureStorage>();
            featureStorage.TryAddFeatureDefinition(simpleExportImportCore.ModuleConstants.Features.SimpleExportImport, featureManagementCore.ModuleConstants.FeatureFilters.Developers);

            // Ensure that any pending migrations are applied
            using (var serviceScope = appBuilder.ApplicationServices.CreateScope())
            {
                using (var dbContext = serviceScope.ServiceProvider.GetRequiredService<VirtoCommerceSimpleExportImportModuleDbContext>())
                {
                    dbContext.Database.EnsureCreated();
                    dbContext.Database.Migrate();
                }
            }
        }

        public void Uninstall()
        {
            // do nothing in here
        }

    }

}
