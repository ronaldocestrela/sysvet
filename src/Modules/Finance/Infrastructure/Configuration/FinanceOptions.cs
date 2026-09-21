namespace Finance.Infrastructure.Configuration;

/// <summary>
/// Finance module configuration; optional database override when not using the shared default connection.
/// </summary>
public class FinanceOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Finance";

    /// <summary>
    /// When set, used instead of <c>ConnectionStrings:DefaultConnection</c> for <see cref="Persistence.FinanceDbContext"/>.
    /// </summary>
    public string? ConnectionString { get; set; }
}
