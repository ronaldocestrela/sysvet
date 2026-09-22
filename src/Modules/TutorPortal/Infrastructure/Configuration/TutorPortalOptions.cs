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

    /// <summary>VAPID public key (URL-safe base64) for Web Push subscriptions.</summary>
    public string? VapidPublicKey { get; set; }

    /// <summary>VAPID private key for signing outbound push payloads.</summary>
    public string? VapidPrivateKey { get; set; }

    /// <summary>VAPID subject (mailto: or https:) required by push services.</summary>
    public string? VapidSubject { get; set; }
}
