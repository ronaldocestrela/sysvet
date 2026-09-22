using Microsoft.Extensions.Options;
using TutorPortal.Application.Abstractions;
using TutorPortal.Infrastructure.Configuration;

namespace TutorPortal.Infrastructure.Push;

/// <summary>
/// Maps <see cref="TutorPortalOptions"/> to application push settings.
/// </summary>
public sealed class TutorPushSettingsAdapter : ITutorPushSettings
{
    /// <inheritdoc />
    public string? VapidPublicKey { get; }

    /// <inheritdoc />
    public string? VapidPrivateKey { get; }

    /// <inheritdoc />
    public string? VapidSubject { get; }

    /// <summary>Reads VAPID fields from module options.</summary>
    public TutorPushSettingsAdapter(IOptions<TutorPortalOptions> options)
    {
        VapidPublicKey = options.Value.VapidPublicKey;
        VapidPrivateKey = options.Value.VapidPrivateKey;
        VapidSubject = options.Value.VapidSubject;
    }
}
