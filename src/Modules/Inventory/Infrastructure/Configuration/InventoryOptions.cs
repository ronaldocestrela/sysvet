namespace Inventory.Infrastructure.Configuration;

/// <summary>
/// Inventory module configuration; optional database override when not using the shared default connection.
/// </summary>
public class InventoryOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Inventory";

    /// <summary>
    /// When set, used instead of <c>ConnectionStrings:DefaultConnection</c> for <see cref="Persistence.InventoryDbContext"/>.
    /// </summary>
    public string? ConnectionString { get; set; }
}
