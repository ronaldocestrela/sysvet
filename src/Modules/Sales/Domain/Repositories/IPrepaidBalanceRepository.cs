using Sales.Domain.Entities;
using Sales.Domain.Enums;

namespace Sales.Domain.Repositories;

/// <summary>Persistence port for prepaid service balances.</summary>
public interface IPrepaidBalanceRepository
{
    Task<PrepaidBalance?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PrepaidBalance?> GetByPetAndServiceAsync(Guid petId, ServiceCode serviceCode, CancellationToken cancellationToken = default);
    Task<PrepaidBalance?> GetByIdForCreditAsync(Guid petId, ServiceCode serviceCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PrepaidBalance>> ListAsync(Guid? tutorId, Guid? petId, ServiceCode? serviceCode, CancellationToken cancellationToken = default);
    void Add(PrepaidBalance balance);
    void Update(PrepaidBalance balance);
}
