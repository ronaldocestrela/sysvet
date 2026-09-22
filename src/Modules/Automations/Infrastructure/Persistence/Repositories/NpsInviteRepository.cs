using Automations.Domain.Entities;
using Automations.Domain.Enums;
using Automations.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Automations.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF implementation of <see cref="INpsInviteRepository"/>.
/// </summary>
public sealed class NpsInviteRepository : INpsInviteRepository
{
    private readonly AutomationsDbContext _dbContext;

    public NpsInviteRepository(AutomationsDbContext dbContext) => _dbContext = dbContext;

    public void Add(NpsInvite invite) => _dbContext.NpsInvites.Add(invite);

    public void Update(NpsInvite invite) => _dbContext.NpsInvites.Update(invite);

    public Task<NpsInvite?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.NpsInvites.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public Task<bool> ExistsForSourceAsync(string sourceType, Guid sourceId, CancellationToken cancellationToken = default) =>
        _dbContext.NpsInvites.AnyAsync(i => i.SourceType == sourceType && i.SourceId == sourceId, cancellationToken);

    public async Task<IReadOnlyList<NpsInvite>> ListRespondedBetweenAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        var invites = await _dbContext.NpsInvites.ToListAsync(cancellationToken);
        return invites
            .Where(i => i.Status == NpsInviteStatus.Responded
                        && i.RespondedAt >= from
                        && i.RespondedAt <= to)
            .OrderByDescending(i => i.RespondedAt)
            .ToList();
    }
}
