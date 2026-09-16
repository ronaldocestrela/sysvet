namespace Core.Domain;

/// <summary>
/// Marks CRM entities that are removed logically instead of physically deleted from the database.
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// When true, the entity is hidden from normal queries and must not be updated.
    /// </summary>
    bool IsDeleted { get; }

    /// <summary>
    /// UTC timestamp when the entity was soft-deleted, or null if still active.
    /// </summary>
    DateTimeOffset? DeletedAt { get; }
}
