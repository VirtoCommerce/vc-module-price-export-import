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

        public static class Settings
        {
            public const int PageSize = 50;
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
