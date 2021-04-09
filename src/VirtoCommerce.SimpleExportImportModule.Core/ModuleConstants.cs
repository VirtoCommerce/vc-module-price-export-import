using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.SimpleExportImportModule.Core
{
    public static class ModuleConstants
    {
        public const int KByte = 1024;

        public const int MByte = 1024 * KByte;

        public static class ValidationErrors
        {
            public const string DuplicateError = "Duplicate";

            public const string NotExistsError = "NotExists";

            public const string AlreadyExistsError = "AlreadyExists";

            public const string ProductMissingError = "ProductMissing";

            public const string FileNotExisted = "file-not-existed";

            public const string NoData = "no-data";

            public const string ExceedingFileMaxSize = "exceeding-file-max-size";

            public const string WrongDelimiter = "wrong-delimiter";

            public const string ExceedingLineLimits = "exceeding-line-limits";

            public const string MissingRequiredColumns = "missing-required-columns";
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
                public static SettingDescriptor ExportLimitOfLines { get; } = new SettingDescriptor
                {
                    Name = "SimpleExportImport.Export.LimitOfLines",
                    GroupName = "SimpleExportImport|Export",
                    ValueType = SettingValueType.PositiveInteger,
                    IsHidden = true,
                    DefaultValue = 10000
                };

                public static SettingDescriptor ImportLimitOfLines { get; } = new SettingDescriptor
                {
                    Name = "SimpleExportImport.Import.LimitOfLines",
                    GroupName = "SimpleExportImport|Import",
                    ValueType = SettingValueType.PositiveInteger,
                    IsHidden = true,
                    DefaultValue = 10000
                };

                public static SettingDescriptor ImportFileMaxSize { get; } = new SettingDescriptor
                {
                    Name = "SimpleExportImport.Import.FileMaxSize",
                    GroupName = "SimpleExportImport|Import",
                    ValueType = SettingValueType.PositiveInteger,
                    IsHidden = true,
                    DefaultValue = 1 // MB
                };


                public static IEnumerable<SettingDescriptor> AllSettings
                {
                    get
                    {
                        return new List<SettingDescriptor>
                               {
                                   ExportLimitOfLines,
                                   ImportLimitOfLines,
                                   ImportFileMaxSize
                               };
                    }
                }
            }
        }
    }
}
