using Core.Application.IntegrationEvents;

namespace Core.Application.Notifications;

/// <summary>
/// Outbound tutor messaging port; Automations module replaces the default null implementation.
/// </summary>
public interface ITutorNotificationChannel
{
    /// <summary>
    /// Whether an external channel (WhatsApp/SMS/e-mail worker) is registered for this deployment.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Delivers a grooming status message to the tutor when enabled.
    /// </summary>
    Task NotifyGroomingStatusAsync(TutorGroomingNotification notification, CancellationToken cancellationToken = default);
}

/// <summary>
/// Payload for tutor grooming status notifications.
/// </summary>
public sealed record TutorGroomingNotification(
    Guid TenantId,
    Guid TutorId,
    Guid PetId,
    Guid GroomingAppointmentId,
    GroomingNotificationKind Kind,
    string TutorPhone,
    string TutorName,
    string PetName);
