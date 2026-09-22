using Automations.Domain.Entities;
using Automations.Domain.Enums;
using Automations.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Automations.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF implementation of <see cref="IMessageJobRepository"/>.
/// </summary>
public sealed class MessageJobRepository : IMessageJobRepository
{
    private readonly AutomationsDbContext _dbContext;

    public MessageJobRepository(AutomationsDbContext dbContext) => _dbContext = dbContext;

    public void Add(MessageJob job) => _dbContext.MessageJobs.Add(job);

    public Task<MessageJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.MessageJobs.FirstOrDefaultAsync(j => j.Id == id, cancellationToken);

    public Task<MessageJob?> GetByIdWithLogsAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.MessageJobs
            .Include(j => j.AttemptLogs)
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);

    public Task<MessageJob?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default) =>
        string.IsNullOrEmpty(idempotencyKey)
            ? Task.FromResult<MessageJob?>(null)
            : _dbContext.MessageJobs.FirstOrDefaultAsync(j => j.IdempotencyKey == idempotencyKey, cancellationToken);

    public async Task<IReadOnlyList<MessageJob>> ListDueAsync(int batchSize, DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        var jobs = await _dbContext.MessageJobs
            .Include(j => j.AttemptLogs)
            .Where(j =>
                (j.Status == MessageJobStatus.Pending || j.Status == MessageJobStatus.Failed)
                && j.NextAttemptAt <= now)
            .OrderBy(j => j.NextAttemptAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        return jobs;
    }

    public async Task<IReadOnlyList<MessageJob>> ListAsync(MessageJobStatus? status, int take, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.MessageJobs.Include(j => j.AttemptLogs).AsQueryable();
        if (status is { } s)
        {
            query = query.Where(j => j.Status == s);
        }

        return await query
            .OrderByDescending(j => j.UpdatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsIdempotencyKeyPrefixSinceAsync(
        string prefix,
        DateTimeOffset since,
        CancellationToken cancellationToken = default)
    {
        var keys = await _dbContext.MessageJobs
            .AsNoTracking()
            .Select(j => new { j.IdempotencyKey, j.UpdatedAt })
            .ToListAsync(cancellationToken);

        return keys.Any(j => j.UpdatedAt >= since && j.IdempotencyKey.StartsWith(prefix, StringComparison.Ordinal));
    }
}
