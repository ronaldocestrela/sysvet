namespace Sales.Infrastructure.Configuration;

/// <summary>
/// Sales module configuration; optional database override when not using the shared default connection.
/// </summary>
public class SalesOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Sales";

    /// <summary>
    /// When set, used instead of <c>ConnectionStrings:DefaultConnection</c> for <see cref="Persistence.SalesDbContext"/>.
    /// </summary>
    public string? ConnectionString { get; set; }
}
