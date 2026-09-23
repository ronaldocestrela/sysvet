using Platform.Domain.Entities;
using Platform.Domain.Repositories;

namespace Platform.Infrastructure.Persistence.Repositories;

/// <inheritdoc />
public sealed class SubscriptionAdjustmentRepository : ISubscriptionAdjustmentRepository
{
    private readonly PlatformDbContext _context;

    /// <summary>Creates the repository.</summary>
    public SubscriptionAdjustmentRepository(PlatformDbContext context) => _context = context;

    /// <inheritdoc />
    public async Task AddAsync(SubscriptionAdjustment adjustment, CancellationToken cancellationToken = default) =>
        await _context.SubscriptionAdjustments.AddAsync(adjustment, cancellationToken);
}
