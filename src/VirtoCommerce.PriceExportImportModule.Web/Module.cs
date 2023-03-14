using System;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.PriceExportImportModule.Core;
using VirtoCommerce.PriceExportImportModule.Core.Models;
using VirtoCommerce.PriceExportImportModule.Core.Services;
using VirtoCommerce.PriceExportImportModule.Data.Services;
using VirtoCommerce.PriceExportImportModule.Data.Validation;
using VirtoCommerce.PricingModule.Data.ExportImport;

namespace VirtoCommerce.PriceExportImportModule.Web
{
    public class Module : IModule, IHasConfiguration
    {
        public ManifestModuleInfo ModuleInfo { get; set; }

        public IConfiguration Configuration { get; set; }

        public void Initialize(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<ICsvPagedPriceDataSourceFactory, CsvPagedPriceDataSourceFactory>();
            serviceCollection.AddTransient<ICsvPriceDataValidator, CsvPriceDataValidator>();
            serviceCollection.AddTransient<ICsvPagedPriceDataImporter, CsvPagedPriceDataImporter>();
            serviceCollection.AddTransient<ICsvPriceImportReporterFactory, CsvPriceImportReporterFactory>();
            serviceCollection.AddSingleton<ImportConfigurationFactory>();

            serviceCollection.AddTransient<IValidator<ImportProductPrice[]>, ImportProductPricesValidator>();
            serviceCollection.AddTransient<Func<TextReader, CsvConfiguration, IReader>>(provider => (textReader, csvConfiguration) => new VcCsvReader(textReader, csvConfiguration));

            serviceCollection.AddOptions<ExportOptions>().Bind(Configuration.GetSection("PriceExportImport:Export")).ValidateDataAnnotations();
            serviceCollection.AddOptions<ImportOptions>().Bind(Configuration.GetSection("PriceExportImport:Import")).ValidateDataAnnotations();
        }

        public void PostInitialize(IApplicationBuilder appBuilder)
        {
            // prices
            AbstractTypeFactory<TabularPrice>.OverrideType<TabularPrice, ExportTabularPrice>();
            AbstractTypeFactory<ExportablePrice>.OverrideType<ExportablePrice, ExportPrice>();

            var settingsRegistrar = appBuilder.ApplicationServices.GetRequiredService<ISettingsRegistrar>();
            settingsRegistrar.RegisterSettings(ModuleConstants.Settings.General.AllSettings, ModuleInfo.Id);

            var settingsManager = appBuilder.ApplicationServices.GetService<ISettingsManager>();
            var priceExportOptions = appBuilder.ApplicationServices.GetService<IOptions<ExportOptions>>().Value;

            settingsManager.SetValue(ModuleConstants.Settings.General.ExportLimitOfLines.Name,
                priceExportOptions.LimitOfLines ?? ModuleConstants.Settings.General.ExportLimitOfLines.DefaultValue);

            var priceImportOptions = appBuilder.ApplicationServices.GetService<IOptions<ImportOptions>>().Value;

            settingsManager.SetValue(ModuleConstants.Settings.General.ImportLimitOfLines.Name,
                priceImportOptions.LimitOfLines ?? ModuleConstants.Settings.General.ImportLimitOfLines.DefaultValue);

            settingsManager.SetValue(ModuleConstants.Settings.General.ImportFileMaxSize.Name,
                priceImportOptions.FileMaxSize ?? ModuleConstants.Settings.General.ImportFileMaxSize.DefaultValue);

            // register permissions
            var permissionsProvider = appBuilder.ApplicationServices.GetRequiredService<IPermissionsRegistrar>();
            permissionsProvider.RegisterPermissions(ModuleConstants.Security.Permissions.AllPermissions.Select(x =>
                new Permission
                {
                    GroupName = "PriceExportImport",
                    ModuleId = ModuleInfo.Id,
                    Name = x
                }).ToArray());
        }

        public void Uninstall()
        {
            // do nothing in here
        }

    }

}
