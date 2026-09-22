using System.Text.Json;
using Microsoft.Extensions.Logging;
using TutorPortal.Application.Abstractions;
using WebPush;

namespace TutorPortal.Infrastructure.Push;

/// <summary>
/// Sends Web Push notifications using VAPID keys from configuration.
/// </summary>
public sealed class WebPushTutorPushSender : ITutorPushSender
{
    private readonly ITutorPushSettings _settings;
    private readonly ILogger<WebPushTutorPushSender> _logger;

    /// <summary>Creates the sender with VAPID settings.</summary>
    public WebPushTutorPushSender(ITutorPushSettings settings, ILogger<WebPushTutorPushSender> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SendAsync(string endpoint, string p256dh, string auth, string title, string body, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.VapidPublicKey)
            || string.IsNullOrWhiteSpace(_settings.VapidPrivateKey)
            || string.IsNullOrWhiteSpace(_settings.VapidSubject))
        {
            return;
        }

        var subscription = new PushSubscription(endpoint, p256dh, auth);
        var vapid = new VapidDetails(_settings.VapidSubject, _settings.VapidPublicKey, _settings.VapidPrivateKey);
        var client = new WebPushClient();
        var payload = JsonSerializer.Serialize(new { title, body });
        try
        {
            await client.SendNotificationAsync(subscription, payload, vapid, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Web Push delivery failed for endpoint {Endpoint}", endpoint);
        }
    }
}
