using Intelligence.Domain.Entities;

namespace Intelligence.Domain.Repositories;

/// <summary>Persistence for profile dashboard layouts.</summary>
public interface IProfileDashboardLayoutRepository
{
    /// <summary>Loads layout by access profile id within the tenant.</summary>
    Task<ProfileDashboardLayout?> GetByAccessProfileIdAsync(Guid accessProfileId, CancellationToken cancellationToken = default);

    /// <summary>Persists a new layout.</summary>
    Task AddAsync(ProfileDashboardLayout layout, CancellationToken cancellationToken = default);
}
