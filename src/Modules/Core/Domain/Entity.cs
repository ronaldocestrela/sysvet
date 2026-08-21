namespace Core.Domain;

/// <summary>
/// Classe base abstrata para todas as entidades de domínio.
/// Identificada de forma única por um Id do tipo Guid.
/// </summary>
public abstract class Entity : IEquatable<Entity>
{
    /// <summary>
    /// Identificador único universal da entidade.
    /// </summary>
    public Guid Id { get; protected set; }

    /// <summary>
    /// Data e hora da última modificação (UTC) usada para Last-Write-Wins.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Token de concorrência para versionamento otimista (evitar lost updates).
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    protected Entity(Guid id)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
    }

    protected Entity()
    {
        Id = Guid.NewGuid();
    }

    public bool Equals(Entity? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return Id == other.Id;
    }

    public override bool Equals(object? obj) => Equals(obj as Entity);

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity? a, Entity? b) =>
        a is null ? b is null : a.Equals(b);

    public static bool operator !=(Entity? a, Entity? b) => !(a == b);
}

/// <summary>
/// Classe base para agregados (Aggregate Roots) no DDD.
/// </summary>
public abstract class AggregateRoot : Entity
{
    protected AggregateRoot(Guid id) : base(id) { }
    protected AggregateRoot() : base() { }
}
