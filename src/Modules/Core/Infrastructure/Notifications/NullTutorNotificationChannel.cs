using Core.Application.Notifications;

namespace Core.Infrastructure.Notifications;

/// <summary>
/// Default no-op channel until the Automations module registers a real provider.
/// </summary>
public sealed class NullTutorNotificationChannel : ITutorNotificationChannel
{
    /// <inheritdoc />
    public bool IsEnabled => false;

    /// <inheritdoc />
    public Task NotifyGroomingStatusAsync(TutorGroomingNotification notification, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
