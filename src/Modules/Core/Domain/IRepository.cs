namespace Core.Domain;

/// <summary>
/// Generic persistence port for aggregate and entity types, keeping the application layer provider-agnostic.
/// </summary>
/// <typeparam name="T">Entity type constrained to <see cref="Entity"/>.</typeparam>
public interface IRepository<T> where T : Entity
{
    /// <summary>
    /// Loads an entity by primary key, or null when not found.
    /// </summary>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all entities (use sparingly; prefer module-specific query methods).
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stages a new entity for insert on the next unit of work commit.
    /// </summary>
    void Add(T entity);

    /// <summary>
    /// Stages an existing entity for update on the next unit of work commit.
    /// </summary>
    void Update(T entity);

    /// <summary>
    /// Stages an entity for deletion on the next unit of work commit.
    /// </summary>
    void Remove(T entity);
}
