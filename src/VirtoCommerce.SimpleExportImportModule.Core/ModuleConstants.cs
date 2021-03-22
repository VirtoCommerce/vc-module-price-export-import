using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.SimpleExportImportModule.Core
{
    public static class ModuleConstants
    {
        public static class Security
        {
            public static class Permissions
            {
                public const string Access = "virtoCommerceSimpleExportImportModule:access";
                public const string Create = "virtoCommerceSimpleExportImportModule:create";
                public const string Read = "virtoCommerceSimpleExportImportModule:read";
                public const string Update = "virtoCommerceSimpleExportImportModule:update";
                public const string Delete = "virtoCommerceSimpleExportImportModule:delete";

                public static string[] AllPermissions { get; } = { Read, Create, Access, Update, Delete };
            }
        }

        public static class Settings
        {
            public static class General
            {
                public static SettingDescriptor VirtoCommerceSimpleExportImportModuleEnabled { get; } = new SettingDescriptor
                {
                    Name = "VirtoCommerceSimpleExportImportModule.VirtoCommerceSimpleExportImportModuleEnabled",
                    GroupName = "VirtoCommerceSimpleExportImportModule|General",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = false
                };

                public static SettingDescriptor VirtoCommerceSimpleExportImportModulePassword { get; } = new SettingDescriptor
                {
                    Name = "VirtoCommerceSimpleExportImportModule.VirtoCommerceSimpleExportImportModulePassword",
                    GroupName = "VirtoCommerceSimpleExportImportModule|Advanced",
                    ValueType = SettingValueType.SecureString,
                    DefaultValue = "qwerty"
                };

                public static IEnumerable<SettingDescriptor> AllSettings
                {
                    get
                    {
                        yield return VirtoCommerceSimpleExportImportModuleEnabled;
                        yield return VirtoCommerceSimpleExportImportModulePassword;
                    }
                }
            }

            public static IEnumerable<SettingDescriptor> AllSettings
            {
                get
                {
                    return General.AllSettings;
                }
            }
        }
    }
}
