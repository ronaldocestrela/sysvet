using Platform.Domain.Entities;

namespace Platform.Domain.Repositories;

/// <summary>Proration adjustment persistence port.</summary>
public interface ISubscriptionAdjustmentRepository
{
    /// <summary>Persists adjustment row.</summary>
    Task AddAsync(SubscriptionAdjustment adjustment, CancellationToken cancellationToken = default);
}
