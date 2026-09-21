namespace Petshop.Infrastructure.Configuration;

/// <summary>
/// Petshop module configuration; optional database override when not using the shared default connection.
/// </summary>
public class PetshopOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Petshop";

    /// <summary>
    /// When set, used instead of <c>ConnectionStrings:DefaultConnection</c> for <see cref="Persistence.PetshopDbContext"/>.
    /// </summary>
    public string? ConnectionString { get; set; }
}
