using Core.Application.Common.Interfaces;
using Core.Domain.Entities;
using Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Core.Infrastructure.Services;

public class IdempotencyService : IIdempotencyService
{
    private readonly CoreDbContext _dbContext;

    public IdempotencyService(CoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> RequestExistsAsync(Guid idempotencyKey, CancellationToken cancellationToken = default)
    {
        return await _dbContext.IdempotencyRecords.AnyAsync(r => r.Id == idempotencyKey, cancellationToken);
    }

    public async Task CreateRequestAsync(Guid idempotencyKey, string commandName, CancellationToken cancellationToken = default)
    {
        var record = IdempotencyRecord.Create(idempotencyKey, commandName);
        _dbContext.IdempotencyRecords.Add(record);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
