using Automations.Domain.Entities;
using Automations.Domain.Enums;

namespace Automations.Domain.Repositories;

/// <summary>
/// Persistence port for outbound message jobs.
/// </summary>
public interface IMessageJobRepository
{
    void Add(MessageJob job);

    Task<MessageJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<MessageJob?> GetByIdWithLogsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<MessageJob?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MessageJob>> ListDueAsync(int batchSize, DateTimeOffset now, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MessageJob>> ListAsync(MessageJobStatus? status, int take, CancellationToken cancellationToken = default);
}
