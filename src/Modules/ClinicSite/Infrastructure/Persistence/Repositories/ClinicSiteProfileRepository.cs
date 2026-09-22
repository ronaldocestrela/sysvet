using ClinicSite.Domain.Entities;
using ClinicSite.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ClinicSite.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF implementation of clinic site profile persistence.
/// </summary>
public sealed class ClinicSiteProfileRepository : IClinicSiteProfileRepository
{
    private readonly ClinicSiteDbContext _dbContext;

    public ClinicSiteProfileRepository(ClinicSiteDbContext dbContext) => _dbContext = dbContext;

    /// <inheritdoc />
    public async Task<ClinicSiteProfile?> GetAsync(CancellationToken cancellationToken = default)
    {
        var profile = await _dbContext.ClinicSiteProfiles
            .FirstOrDefaultAsync(p => p.Key == ClinicSiteProfile.SingletonKey, cancellationToken);
        if (profile is null)
        {
            return null;
        }

        await _dbContext.Entry(profile).Collection<ClinicSiteServiceItem>("_services").LoadAsync(cancellationToken);
        await _dbContext.Entry(profile).Collection<ClinicSiteTeamMember>("_team").LoadAsync(cancellationToken);
        await _dbContext.Entry(profile).Collection<ClinicSiteOpeningHours>("_hours").LoadAsync(cancellationToken);
        return profile;
    }

    /// <inheritdoc />
    public async Task AddAsync(ClinicSiteProfile profile, CancellationToken cancellationToken = default)
    {
        await _dbContext.ClinicSiteProfiles.AddAsync(profile, cancellationToken);
        await SyncChildrenAsync(profile, cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(ClinicSiteProfile profile, CancellationToken cancellationToken = default)
    {
        _dbContext.ClinicSiteProfiles.Update(profile);
        await SyncChildrenAsync(profile, cancellationToken);
    }

    private async Task SyncChildrenAsync(ClinicSiteProfile profile, CancellationToken cancellationToken)
    {
        await _dbContext.ClinicSiteServiceItems
            .Where(x => x.ClinicSiteProfileId == profile.Id)
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.ClinicSiteTeamMembers
            .Where(x => x.ClinicSiteProfileId == profile.Id)
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.ClinicSiteOpeningHours
            .Where(x => x.ClinicSiteProfileId == profile.Id)
            .ExecuteDeleteAsync(cancellationToken);

        foreach (var service in profile.Services)
        {
            await _dbContext.ClinicSiteServiceItems.AddAsync(service, cancellationToken);
        }

        foreach (var member in profile.Team)
        {
            await _dbContext.ClinicSiteTeamMembers.AddAsync(member, cancellationToken);
        }

        foreach (var hours in profile.Hours)
        {
            await _dbContext.ClinicSiteOpeningHours.AddAsync(hours, cancellationToken);
        }
    }
}
