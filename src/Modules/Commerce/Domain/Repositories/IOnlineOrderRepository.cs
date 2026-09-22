using Commerce.Domain.Entities;

namespace Commerce.Domain.Repositories;

/// <summary>Persistence port for online orders.</summary>
public interface IOnlineOrderRepository
{
    Task<OnlineOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OnlineOrder?> GetByExternalOrderIdAsync(string externalOrderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OnlineOrder>> ListAsync(CancellationToken cancellationToken = default);
    void Add(OnlineOrder order);
    void Update(OnlineOrder order);
}
