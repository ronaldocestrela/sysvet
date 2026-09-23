using Core.Domain;

namespace Core.Application.Privacy;

/// <summary>
/// Propagates tutor anonymization tombstones to module-local PII copies.
/// </summary>
public interface IPersonalDataErasureContributor
{
    /// <summary>Stable module key (e.g. Sales, Fiscal).</summary>
    string ModuleKey { get; }

    /// <summary>Replaces or clears tutor-linked personal data copies.</summary>
    Task<Result> EraseForTutorAsync(PersonalDataErasureContext context, CancellationToken cancellationToken);
}
