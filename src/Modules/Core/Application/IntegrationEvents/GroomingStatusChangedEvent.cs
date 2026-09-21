using MediatR;

namespace Core.Application.IntegrationEvents;

/// <summary>
/// Published after grooming lifecycle transitions for realtime UI and tutor notification channels.
/// </summary>
public sealed class GroomingStatusChangedEvent : INotification
{
    public Guid TenantId { get; }
    public Guid GroomingAppointmentId { get; }
    public Guid PetId { get; }
    public Guid TutorId { get; }
    public GroomingNotificationKind Kind { get; }
    public string Status { get; }
    public DateTimeOffset OccurredOn { get; }

    public GroomingStatusChangedEvent(
        Guid tenantId,
        Guid groomingAppointmentId,
        Guid petId,
        Guid tutorId,
        GroomingNotificationKind kind,
        string status,
        DateTimeOffset occurredOn)
    {
        TenantId = tenantId;
        GroomingAppointmentId = groomingAppointmentId;
        PetId = petId;
        TutorId = tutorId;
        Kind = kind;
        Status = status;
        OccurredOn = occurredOn;
    }
}

/// <summary>
/// Tutor-facing grooming notification classification (maps to message templates in Automations 8.x).
/// </summary>
public enum GroomingNotificationKind
{
    Started = 1,
    ReadyForPickup = 2,
    Completed = 3
}
