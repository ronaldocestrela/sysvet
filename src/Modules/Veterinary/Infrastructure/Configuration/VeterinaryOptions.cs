namespace Veterinary.Infrastructure.Configuration;

/// <summary>
/// Veterinary module configuration; optional database override when not using the shared default connection.
/// </summary>
public class VeterinaryOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Veterinary";

    /// <summary>
    /// When set, used instead of <c>ConnectionStrings:DefaultConnection</c> for <see cref="Persistence.VeterinaryDbContext"/>.
    /// </summary>
    public string? ConnectionString { get; set; }
}
