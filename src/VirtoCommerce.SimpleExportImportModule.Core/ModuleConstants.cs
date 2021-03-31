namespace VirtoCommerce.SimpleExportImportModule.Core
{
    public static class ModuleConstants
    {
        public const long Byte = 8;

        public const long KByte = 1024 * Byte;

        public const long MByte = 1024 * KByte;

        public static class ValidationErrors
        {
            public const string DuplicateError = "Duplicate";

            public const string AlreadyExistsError = "AlreadyExists";

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

        public static class Settings
        {
            public const int PageSize = 50;

            public const long FileMaxSize = MByte;

            public const long ImportLimitOfLines = 10000;
        }

        public static class Security
        {
            public static class Permissions
            {
                public const string ImportAccess = "import:access";

                public static string[] AllPermissions { get; } = { ImportAccess };
            }
        }
    }
}
