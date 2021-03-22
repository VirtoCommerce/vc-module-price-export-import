namespace VirtoCommerce.SimpleExportImportModule.Core
{
    public static class ModuleConstants
    {
        public static class Security
        {
            public static class Permissions
            {
                public const string Import = "virtoCommerceSimpleExportImportModule:import";

                public static string[] AllPermissions { get; } = { Import };
            }
        }
    }
}
