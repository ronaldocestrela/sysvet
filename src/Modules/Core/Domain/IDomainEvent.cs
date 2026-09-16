namespace Core.Domain;

/// <summary>
/// Marks a domain occurrence raised by an aggregate root, distinct from cross-module integration notifications.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// UTC timestamp when the event occurred in the domain model.
    /// </summary>
    DateTimeOffset OccurredOn { get; }
}
