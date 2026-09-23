namespace Platform.Domain.Catalog;

/// <summary>Stable plan and add-on codes for seed and Super Admin API (9.3).</summary>
public static class CatalogCodes
{
    /// <summary>Base plan identifiers.</summary>
    public static class Plans
    {
        public const string Starter = "Starter";
        public const string Pro = "Pro";
        public const string Hospital24h = "Hospital24h";
    }

    /// <summary>Add-on product identifiers.</summary>
    public static class AddOns
    {
        public const string Estetica = "Estetica";
        public const string Fiscal = "Fiscal";
        public const string Automacao = "Automacao";
        public const string PdvOffline = "PdvOffline";

        /// <summary>Operational dashboards add-on (roadmap 10.1).</summary>
        public const string Intelligence = "Intelligence";
    }
}
