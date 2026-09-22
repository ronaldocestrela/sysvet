using Automations.Domain.Entities;
using Automations.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Automations.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF implementation of tenant Automations settings.
/// </summary>
public sealed class AutomationsSettingsRepository : IAutomationsSettingsRepository
{
    private readonly AutomationsDbContext _dbContext;

    public AutomationsSettingsRepository(AutomationsDbContext dbContext) => _dbContext = dbContext;

    public void Add(AutomationsSettings settings) => _dbContext.Set<AutomationsSettings>().Add(settings);

    public Task<AutomationsSettings?> GetSingletonAsync(CancellationToken cancellationToken = default) =>
        _dbContext.Set<AutomationsSettings>()
            .FirstOrDefaultAsync(s => s.Key == AutomationsSettings.SingletonKey, cancellationToken);

    public void Update(AutomationsSettings settings) => _dbContext.Set<AutomationsSettings>().Update(settings);
}
