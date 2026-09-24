using Microsoft.EntityFrameworkCore;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;

namespace Platform.Infrastructure.Persistence.Repositories;

/// <summary>EF implementation of status incident persistence.</summary>
public sealed class StatusIncidentRepository : IStatusIncidentRepository
{
    private readonly PlatformDbContext _context;

    /// <summary>Creates the repository.</summary>
    public StatusIncidentRepository(PlatformDbContext context) => _context = context;

    /// <inheritdoc />
    public Task<StatusIncident?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.StatusIncidents.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<StatusIncident>> ListOpenAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _context.StatusIncidents
            .AsNoTracking()
            .Where(i => i.ResolvedAt == null)
            .ToListAsync(cancellationToken);
        return rows.OrderByDescending(i => i.StartedAt).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<StatusIncident>> ListRecentAsync(int take, CancellationToken cancellationToken = default)
    {
        var limit = Math.Clamp(take, 1, 100);
        var rows = await _context.StatusIncidents.AsNoTracking().ToListAsync(cancellationToken);
        return rows
            .OrderBy(i => i.ResolvedAt.HasValue)
            .ThenByDescending(i => i.StartedAt)
            .Take(limit)
            .ToList();
    }

    /// <inheritdoc />
    public async Task AddAsync(StatusIncident incident, CancellationToken cancellationToken = default)
    {
        await _context.StatusIncidents.AddAsync(incident, cancellationToken);
    }
}
