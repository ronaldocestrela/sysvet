namespace TutorPortal.Application.Abstractions;

/// <summary>
/// Read-only CRM lookup used to validate tutor self-registration against existing clinic records.
/// </summary>
public interface ICrmTutorLookup
{
    /// <summary>
    /// Resolves an active CRM tutor when both email and CPF match the same aggregate.
    /// </summary>
    Task<CrmTutorMatch?> FindActiveTutorByEmailAndCpfAsync(string email, string cpf, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads an active CRM tutor by identifier.
    /// </summary>
    Task<CrmTutorMatch?> GetActiveTutorByIdAsync(Guid tutorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads basic pet identifiers for a tutor within the current tenant.
    /// </summary>
    Task<IReadOnlyList<CrmPetSummary>> GetPetsForTutorAsync(Guid tutorId, CancellationToken cancellationToken = default);
}

/// <summary>
/// CRM tutor data required for portal registration and profile.
/// </summary>
public sealed record CrmTutorMatch(Guid TutorId, string Name, string Email);

/// <summary>
/// Minimal pet row for tutor portal home.
/// </summary>
public sealed record CrmPetSummary(Guid Id, string Name);
