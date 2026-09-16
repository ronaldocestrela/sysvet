using Core.Domain;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Core.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IUserPreferenceRepository"/>.
/// </summary>
public sealed class UserPreferenceRepository : Repository<UserPreference>, IUserPreferenceRepository
{
    public UserPreferenceRepository(CoreDbContext dbContext) : base(dbContext)
    {
    }

    /// <inheritdoc />
    public async Task<UserPreference?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
    }
}
