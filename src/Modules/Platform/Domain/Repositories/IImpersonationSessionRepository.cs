using Platform.Domain.Entities;

namespace Platform.Domain.Repositories;

/// <summary>Persistence port for impersonation sessions (9.6).</summary>
public interface IImpersonationSessionRepository
{
    /// <summary>Gets session by id.</summary>
    Task<ImpersonationSession?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>Adds a session.</summary>
    Task AddAsync(ImpersonationSession session, CancellationToken cancellationToken = default);
}
