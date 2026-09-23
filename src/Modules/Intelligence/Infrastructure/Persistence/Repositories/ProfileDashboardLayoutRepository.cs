using Intelligence.Domain.Entities;
using Intelligence.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Intelligence.Infrastructure.Persistence.Repositories;

/// <summary>EF Core repository for profile dashboard layouts.</summary>
public sealed class ProfileDashboardLayoutRepository : IProfileDashboardLayoutRepository
{
    private readonly IntelligenceDbContext _dbContext;

    /// <summary>Creates the repository.</summary>
    public ProfileDashboardLayoutRepository(IntelligenceDbContext dbContext) => _dbContext = dbContext;

    /// <inheritdoc />
    public Task<ProfileDashboardLayout?> GetByAccessProfileIdAsync(Guid accessProfileId, CancellationToken cancellationToken = default) =>
        _dbContext.ProfileDashboardLayouts
            .FirstOrDefaultAsync(l => l.AccessProfileId == accessProfileId, cancellationToken);

    /// <inheritdoc />
    public Task AddAsync(ProfileDashboardLayout layout, CancellationToken cancellationToken = default) =>
        _dbContext.ProfileDashboardLayouts.AddAsync(layout, cancellationToken).AsTask();
}
