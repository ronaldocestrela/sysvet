namespace Core.Application.Common.Interfaces;

public interface IIdempotencyService
{
    Task<bool> RequestExistsAsync(Guid idempotencyKey, CancellationToken cancellationToken = default);
    Task CreateRequestAsync(Guid idempotencyKey, string commandName, CancellationToken cancellationToken = default);
}
