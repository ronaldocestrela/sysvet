namespace Core.Domain.Events;

/// <summary>
/// Raised when a new tutor aggregate is successfully created in the domain.
/// </summary>
public sealed record TutorRegisteredDomainEvent(Guid TutorId, DateTimeOffset OccurredOn) : IDomainEvent;
