using Microsoft.EntityFrameworkCore;
using TutorPortal.Domain.Entities;
using TutorPortal.Domain.Repositories;

namespace TutorPortal.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core repository for tutor push subscriptions.
/// </summary>
public sealed class TutorPushSubscriptionRepository : ITutorPushSubscriptionRepository
{
    private readonly TutorPortalDbContext _dbContext;

    /// <summary>Creates the repository bound to the module context.</summary>
    public TutorPushSubscriptionRepository(TutorPortalDbContext dbContext) => _dbContext = dbContext;

    /// <inheritdoc />
    public async Task AddAsync(TutorPushSubscription subscription, CancellationToken cancellationToken = default) =>
        await _dbContext.TutorPushSubscriptions.AddAsync(subscription, cancellationToken);

    /// <inheritdoc />
    public async Task<TutorPushSubscription?> GetByUserAndEndpointAsync(string userId, string endpoint, CancellationToken cancellationToken = default) =>
        await _dbContext.TutorPushSubscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Endpoint == endpoint, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<TutorPushSubscription>> ListByUserIdAsync(string userId, CancellationToken cancellationToken = default) =>
        await _dbContext.TutorPushSubscriptions.Where(s => s.UserId == userId).ToListAsync(cancellationToken);

    /// <inheritdoc />
    public void Remove(TutorPushSubscription subscription) => _dbContext.TutorPushSubscriptions.Remove(subscription);
}
