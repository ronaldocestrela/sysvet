namespace Core.Domain;

/// <summary>
/// Marks entities whose personal identifiers can be irreversibly tombstoned for LGPD erasure.
/// </summary>
public interface IAnonymizable
{
    /// <summary>
    /// Whether personal identifiers were replaced with tombstone values.
    /// </summary>
    bool IsAnonymized { get; }

    /// <summary>
    /// UTC timestamp of anonymization, if applied.
    /// </summary>
    DateTimeOffset? AnonymizedAt { get; }
}
