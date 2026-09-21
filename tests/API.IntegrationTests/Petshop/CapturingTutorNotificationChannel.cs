using Core.Application.Notifications;

namespace API.IntegrationTests.Petshop;

/// <summary>
/// Test double that records tutor notifications for grooming acceptance tests.
/// </summary>
public sealed class CapturingTutorNotificationChannel : ITutorNotificationChannel
{
    private readonly object _lock = new();
    private readonly List<TutorGroomingNotification> _notifications = new();

    public IReadOnlyList<TutorGroomingNotification> Notifications
    {
        get
        {
            lock (_lock)
            {
                return _notifications.ToList();
            }
        }
    }

    public bool IsEnabled => true;

    public Task NotifyGroomingStatusAsync(TutorGroomingNotification notification, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _notifications.Add(notification);
        }

        return Task.CompletedTask;
    }

    public void Clear()
    {
        lock (_lock)
        {
            _notifications.Clear();
        }
    }
}
