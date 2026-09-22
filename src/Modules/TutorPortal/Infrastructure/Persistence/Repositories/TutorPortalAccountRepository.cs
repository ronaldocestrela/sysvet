using Microsoft.EntityFrameworkCore;
using TutorPortal.Domain.Entities;
using TutorPortal.Domain.Repositories;

namespace TutorPortal.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="ITutorPortalAccountRepository"/>.
/// </summary>
public sealed class TutorPortalAccountRepository : ITutorPortalAccountRepository
{
    private readonly TutorPortalDbContext _dbContext;

    public TutorPortalAccountRepository(TutorPortalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public void Add(TutorPortalAccount account) => _dbContext.TutorPortalAccounts.Add(account);

    /// <inheritdoc />
    public Task<TutorPortalAccount?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default) =>
        _dbContext.TutorPortalAccounts.FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken);

    /// <inheritdoc />
    public Task<TutorPortalAccount?> GetByTutorIdAsync(Guid tutorId, CancellationToken cancellationToken = default) =>
        _dbContext.TutorPortalAccounts.FirstOrDefaultAsync(a => a.TutorId == tutorId, cancellationToken);
}
