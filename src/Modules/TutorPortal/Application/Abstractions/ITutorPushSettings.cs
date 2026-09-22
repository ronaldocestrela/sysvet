namespace TutorPortal.Application.Abstractions;

/// <summary>
/// Server-side Web Push (VAPID) configuration exposed to application handlers.
/// </summary>
public interface ITutorPushSettings
{
    /// <summary>VAPID public key for browser subscription, when configured.</summary>
    string? VapidPublicKey { get; }

    /// <summary>VAPID private key used to sign outbound push messages.</summary>
    string? VapidPrivateKey { get; }

    /// <summary>Contact URI (mailto: or https:) required by VAPID.</summary>
    string? VapidSubject { get; }
}
