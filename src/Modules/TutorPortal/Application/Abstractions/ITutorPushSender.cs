namespace TutorPortal.Application.Abstractions;

/// <summary>
/// Sends Web Push notifications to registered tutor browsers.
/// </summary>
public interface ITutorPushSender
{
    /// <summary>
    /// Delivers a notification payload to the given push endpoint.
    /// </summary>
    Task SendAsync(string endpoint, string p256dh, string auth, string title, string body, CancellationToken cancellationToken = default);
}
