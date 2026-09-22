using TutorPortal.Application.Abstractions;

namespace TutorPortal.Infrastructure.Push;

/// <summary>
/// No-op push sender used when VAPID is not configured or in tests.
/// </summary>
public sealed class FakeTutorPushSender : ITutorPushSender
{
    /// <inheritdoc />
    public Task SendAsync(string endpoint, string p256dh, string auth, string title, string body, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
