using Core.Domain;

namespace Petshop.Domain.Events;

/// <summary>
/// Raised when a grooming appointment is ready for tutor pickup (for tutor notifications).
/// </summary>
public sealed record GroomingReadyForPickupDomainEvent(
    Guid GroomingAppointmentId,
    Guid PetId,
    Guid TutorId,
    DateTimeOffset OccurredOn) : IDomainEvent;
