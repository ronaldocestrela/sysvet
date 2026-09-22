using Automations.Domain.Entities;
using Automations.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Automations.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF implementation of tutor messaging preferences.
/// </summary>
public sealed class TutorMessagingPreferenceRepository : ITutorMessagingPreferenceRepository
{
    private readonly AutomationsDbContext _dbContext;

    public TutorMessagingPreferenceRepository(AutomationsDbContext dbContext) => _dbContext = dbContext;

    public void Add(TutorMessagingPreference preference) => _dbContext.Set<TutorMessagingPreference>().Add(preference);

    public Task<TutorMessagingPreference?> GetByTutorIdAsync(Guid tutorId, CancellationToken cancellationToken = default) =>
        _dbContext.Set<TutorMessagingPreference>().FirstOrDefaultAsync(p => p.TutorId == tutorId, cancellationToken);

    public void Update(TutorMessagingPreference preference) => _dbContext.Set<TutorMessagingPreference>().Update(preference);
}
