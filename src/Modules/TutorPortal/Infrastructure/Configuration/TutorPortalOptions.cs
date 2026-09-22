namespace TutorPortal.Infrastructure.Configuration;

/// <summary>
/// TutorPortal module configuration; optional database override when not using the shared default connection.
/// </summary>
public class TutorPortalOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "TutorPortal";

    /// <summary>
    /// When set, used instead of <c>ConnectionStrings:DefaultConnection</c> for <see cref="Persistence.TutorPortalDbContext"/>.
    /// </summary>
    public string? ConnectionString { get; set; }
}
