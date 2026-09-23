using Microsoft.EntityFrameworkCore;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;

namespace Platform.Infrastructure.Persistence.Repositories;

/// <inheritdoc />
public sealed class ImpersonationSessionRepository : IImpersonationSessionRepository
{
    private readonly PlatformDbContext _context;

    /// <summary>Creates the repository.</summary>
    public ImpersonationSessionRepository(PlatformDbContext context) => _context = context;

    /// <inheritdoc />
    public Task<ImpersonationSession?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken = default) =>
        _context.ImpersonationSessions.FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(ImpersonationSession session, CancellationToken cancellationToken = default) =>
        await _context.ImpersonationSessions.AddAsync(session, cancellationToken);
}
