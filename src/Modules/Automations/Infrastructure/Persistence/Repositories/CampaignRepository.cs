using Automations.Domain.Entities;
using Automations.Domain.Enums;
using Automations.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Automations.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF implementation of <see cref="ICampaignRepository"/>.
/// </summary>
public sealed class CampaignRepository : ICampaignRepository
{
    private readonly AutomationsDbContext _dbContext;

    public CampaignRepository(AutomationsDbContext dbContext) => _dbContext = dbContext;

    public void Add(Campaign campaign) => _dbContext.Campaigns.Add(campaign);

    public void Update(Campaign campaign) => _dbContext.Campaigns.Update(campaign);

    public Task<Campaign?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Campaigns.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<Campaign?> GetByIdWithRunsAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Campaigns
            .Include(c => c.Runs)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Campaign>> ListAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Campaigns.OrderByDescending(c => c.UpdatedAt).ToListAsync(cancellationToken);

    public void AddRun(CampaignRun run) => _dbContext.CampaignRuns.Add(run);

    public async Task<Campaign?> GetActiveBySegmentAsync(CampaignSegmentKind segmentKind, CancellationToken cancellationToken = default)
    {
        var campaigns = await _dbContext.Campaigns.ToListAsync(cancellationToken);
        return campaigns
            .Where(c => c.SegmentKind == segmentKind && c.Status == CampaignStatus.Active)
            .OrderByDescending(c => c.UpdatedAt)
            .FirstOrDefault();
    }
}
