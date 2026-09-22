using TutorPortal.Domain.Entities;

namespace TutorPortal.Domain.Repositories;

/// <summary>
/// Persistence port for tutor Web Push subscriptions.
/// </summary>
public interface ITutorPushSubscriptionRepository
{
    /// <summary>Finds a subscription by user and endpoint URL.</summary>
    Task<TutorPushSubscription?> GetByUserAndEndpointAsync(string userId, string endpoint, CancellationToken cancellationToken = default);

    /// <summary>Lists all subscriptions for a tutor portal user.</summary>
    Task<IReadOnlyList<TutorPushSubscription>> ListByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>Persists a new subscription.</summary>
    Task AddAsync(TutorPushSubscription subscription, CancellationToken cancellationToken = default);

    /// <summary>Removes a subscription row.</summary>
    void Remove(TutorPushSubscription subscription);
}
