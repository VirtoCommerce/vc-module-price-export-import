using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VirtoCommerce.FeatureManagementModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.PricingModule.Data.ExportImport;
using VirtoCommerce.SimpleExportImportModule.Core;
using VirtoCommerce.SimpleExportImportModule.Core.Models;
using VirtoCommerce.SimpleExportImportModule.Core.Services;
using VirtoCommerce.SimpleExportImportModule.Data.Repositories;
using VirtoCommerce.SimpleExportImportModule.Data.Services;
using VirtoCommerce.SimpleExportImportModule.Data.Validation;
using featureManagementCore = VirtoCommerce.FeatureManagementModule.Core;
using simpleExportImportCore = VirtoCommerce.SimpleExportImportModule.Core;

namespace VirtoCommerce.SimpleExportImportModule.Web
{
    public class Module : IModule, IHasConfiguration
    {
        public ManifestModuleInfo ModuleInfo { get; set; }

        public IConfiguration Configuration { get; set; }

        public void Initialize(IServiceCollection serviceCollection)
        {
            // database initialization
            var connectionString = Configuration.GetConnectionString("VirtoCommerce.SimpleExportImport") ?? Configuration.GetConnectionString("VirtoCommerce");
            serviceCollection.AddDbContext<VirtoCommerceSimpleExportImportModuleDbContext>(options => options.UseSqlServer(connectionString));

            serviceCollection.AddTransient<ICsvPagedPriceDataSourceFactory, CsvPagedPriceDataSourceFactory>();
            serviceCollection.AddTransient<ICsvPriceDataValidator, CsvPriceDataValidator>();
            serviceCollection.AddTransient<ICsvPagedPriceDataImporter, CsvPagedPriceDataImporter>();
            serviceCollection.AddTransient<ICsvPriceImportReporter, CsvPriceImportReporter>();

            serviceCollection.AddTransient<IValidator<ImportProductPrice[]>, ImportProductPricesValidator>();

            serviceCollection.AddOptions<ExportOptions>().Bind(Configuration.GetSection("SimpleExportImport:Export")).ValidateDataAnnotations();
            serviceCollection.AddOptions<ImportOptions>().Bind(Configuration.GetSection("SimpleExportImport:Import")).ValidateDataAnnotations();
        }

        public void PostInitialize(IApplicationBuilder appBuilder)
        {
            // prices
            AbstractTypeFactory<TabularPrice>.OverrideType<TabularPrice, ExportTabularPrice>();
            AbstractTypeFactory<ExportablePrice>.OverrideType<ExportablePrice, ExportPrice>();

            var settingsRegistrar = appBuilder.ApplicationServices.GetRequiredService<ISettingsRegistrar>();
            settingsRegistrar.RegisterSettings(ModuleConstants.Settings.General.AllSettings, ModuleInfo.Id);

            var settingsManager = appBuilder.ApplicationServices.GetService<ISettingsManager>();
            var simpleExportOptions = appBuilder.ApplicationServices.GetService<IOptions<ExportOptions>>().Value;

            settingsManager.SetValue(ModuleConstants.Settings.General.ExportLimitOfLines.Name,
                simpleExportOptions.LimitOfLines ?? ModuleConstants.Settings.General.ExportLimitOfLines.DefaultValue);

            var simpleImportOptions = appBuilder.ApplicationServices.GetService<IOptions<ImportOptions>>().Value;

            settingsManager.SetValue(ModuleConstants.Settings.General.ImportLimitOfLines.Name,
                simpleImportOptions.LimitOfLines ?? ModuleConstants.Settings.General.ImportLimitOfLines.DefaultValue);

            settingsManager.SetValue(ModuleConstants.Settings.General.ImportFileMaxSize.Name,
                simpleImportOptions.FileMaxSize ?? ModuleConstants.Settings.General.ImportFileMaxSize.DefaultValue);

            // register permissions
            var permissionsProvider = appBuilder.ApplicationServices.GetRequiredService<IPermissionsRegistrar>();
            permissionsProvider.RegisterPermissions(ModuleConstants.Security.Permissions.AllPermissions.Select(x =>
                new Permission
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
