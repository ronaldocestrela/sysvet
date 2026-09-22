using Automations.Domain.Entities;

namespace Automations.Domain.Repositories;

/// <summary>
/// Persistence port for tenant Automations settings singleton.
/// </summary>
public interface IAutomationsSettingsRepository
{
    Task<AutomationsSettings?> GetSingletonAsync(CancellationToken cancellationToken = default);

    void Add(AutomationsSettings settings);

    void Update(AutomationsSettings settings);
}
