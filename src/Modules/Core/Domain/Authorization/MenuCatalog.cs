namespace Core.Domain.Authorization;

/// <summary>
/// Maps navigation menu keys to the minimum permission required to display the entry.
/// </summary>
public static class MenuCatalog
{
    private static readonly IReadOnlyDictionary<string, string> MenuToPermission = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["users"] = Permissions.UsersRead,
        ["profiles"] = Permissions.ProfilesRead,
        ["tutors"] = Permissions.TutorsRead,
        ["pets"] = Permissions.PetsRead,
        ["appointments"] = Permissions.AppointmentsRead,
        ["vaccines"] = Permissions.VaccinesRead,
        ["hospitalizations"] = Permissions.HospitalizationsRead,
        ["quotes"] = Permissions.ClinicalQuotesRead,
        ["inventory"] = Permissions.ProductsRead,
        ["suppliers"] = Permissions.ProductsRead,
        ["purchase-imports"] = Permissions.PurchaseImportsRead,
        ["stock"] = Permissions.StockRead,
        ["stock-alerts"] = Permissions.StockRead,
        ["inventory-counts"] = Permissions.StockRead,
        ["sales"] = Permissions.SalesRead,
        ["cash"] = Permissions.CashRegisterRead,
        ["audit"] = Permissions.AuditRead
    };

    /// <summary>
    /// All menu keys in display order for clients.
    /// </summary>
    public static IReadOnlyList<string> AllMenuKeys { get; } = MenuToPermission.Keys.ToList();

    /// <summary>
    /// Returns menu keys the given permission set may display.
    /// </summary>
    public static IReadOnlyList<string> ResolveMenus(IEnumerable<string> grantedPermissions)
    {
        var set = grantedPermissions.ToHashSet(StringComparer.Ordinal);
        return MenuToPermission
            .Where(kvp => set.Contains(kvp.Value))
            .Select(kvp => kvp.Key)
            .ToList();
    }

    /// <summary>
    /// Returns the permission required for a menu key, or null when unknown.
    /// </summary>
    public static string? GetRequiredPermission(string menuKey) =>
        MenuToPermission.TryGetValue(menuKey, out var permission) ? permission : null;
}
