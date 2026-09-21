using Core.Domain;

namespace Petshop.Domain.Events;

/// <summary>
/// Raised when a grooming appointment is completed (for future tutor notifications).
/// </summary>
public sealed record GroomingCompletedDomainEvent(
    Guid GroomingAppointmentId,
    Guid PetId,
    Guid TutorId,
    DateTimeOffset OccurredOn) : IDomainEvent;
