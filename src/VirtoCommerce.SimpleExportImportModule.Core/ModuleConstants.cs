using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.SimpleExportImportModule.Core
{
    public static class ModuleConstants
    {
        public static class ValidationErrors
        {
            public const string DuplicateError = "Duplicate";
            public const string AlreadyExistsError = "AlreadyExists";
        }

        public static class Features
        {
            public const string SimpleExportImport = "SimpleExportImport";
        }

        public static class Security
        {
            public static class Permissions
            {
                public const string ImportAccess = "import:access";

                public static string[] AllPermissions { get; } = { ImportAccess };
            }
        }

        public static class Settings
        {
            public const int PageSize = 50;

            public static class General
            {
                public static SettingDescriptor SimpleExportLimitOfLines { get; } = new SettingDescriptor
                {
                    Name = "SimpleExportImport.SimpleExport.LimitOfLines",
                    GroupName = "SimpleExportImport|SimpleExport",
                    ValueType = SettingValueType.PositiveInteger,
                    IsHidden = true,
                    DefaultValue = 10000
                };

                public static IEnumerable<SettingDescriptor> AllSettings
                {
                    get
                    {
                        return new List<SettingDescriptor>
                               {
                                   SimpleExportLimitOfLines
                               };
                    }
                }
            }
        }
    }
}
