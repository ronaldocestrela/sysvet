namespace Core.Application.Privacy;

/// <summary>
/// Optional module slice for tutor personal-data export (LGPD access/portability).
/// </summary>
public interface IPersonalDataExportContributor
{
    /// <summary>Stable module key (e.g. Sales, Fiscal).</summary>
    string ModuleKey { get; }

    /// <summary>Returns JSON-serializable slices keyed by logical name.</summary>
    Task<IReadOnlyDictionary<string, object?>> GetSlicesAsync(Guid tutorId, CancellationToken cancellationToken);
}
