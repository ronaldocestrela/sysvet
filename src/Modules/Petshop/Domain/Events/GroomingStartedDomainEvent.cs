using Core.Domain;

namespace Petshop.Domain.Events;

/// <summary>
/// Raised when a grooming appointment enters in-progress (for future tutor notifications).
/// </summary>
public sealed record GroomingStartedDomainEvent(
    Guid GroomingAppointmentId,
    Guid PetId,
    Guid TutorId,
    DateTimeOffset OccurredOn) : IDomainEvent;
