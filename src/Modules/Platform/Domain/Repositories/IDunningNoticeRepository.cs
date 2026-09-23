using Platform.Domain.Entities;

namespace Platform.Domain.Repositories;

/// <summary>Dunning notice persistence port (9.5).</summary>
public interface IDunningNoticeRepository
{
    /// <summary>Lists sent notice keys for tenant invoice.</summary>
    Task<IReadOnlySet<string>> ListSentKeysAsync(
        Guid tenantId,
        Guid invoiceId,
        CancellationToken cancellationToken = default);

    /// <summary>Persists a notice.</summary>
    Task AddAsync(DunningNotice notice, CancellationToken cancellationToken = default);
}
