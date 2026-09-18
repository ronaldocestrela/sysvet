using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;

namespace Veterinary.Infrastructure.Persistence.Repositories;

public sealed class VaccineProtocolRepository : IVaccineProtocolRepository
{
    private readonly VeterinaryDbContext _dbContext;

    public VaccineProtocolRepository(VeterinaryDbContext dbContext) => _dbContext = dbContext;

    public async Task AddAsync(VaccineProtocol protocol, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<VaccineProtocol>().AddAsync(protocol, cancellationToken);

    public async Task<VaccineProtocol?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<VaccineProtocol>()
            .Include(p => p.Doses)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<List<VaccineProtocol>> ListAsync(PetSpecies? species, bool activeOnly, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<VaccineProtocol>().AsNoTracking().Include(p => p.Doses).AsQueryable();
        if (species.HasValue)
        {
            query = query.Where(p => p.Species == species.Value);
        }

        if (activeOnly)
        {
            query = query.Where(p => p.IsActive);
        }

        return await query.OrderBy(p => p.Name).ToListAsync(cancellationToken);
    }

    public async Task<(VaccineProtocol Protocol, VaccineProtocolDose Dose)?> FindDoseAsync(Guid protocolDoseId, CancellationToken cancellationToken = default)
    {
        var dose = await _dbContext.Set<VaccineProtocolDose>()
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == protocolDoseId, cancellationToken);
        if (dose is null)
        {
            return null;
        }

        var protocol = await GetByIdAsync(dose.VaccineProtocolId, cancellationToken);
        if (protocol is null)
        {
            return null;
        }

        var matched = protocol.Doses.FirstOrDefault(d => d.Id == protocolDoseId);
        return matched is null ? null : (protocol, matched);
    }

    public void Update(VaccineProtocol protocol) => _dbContext.Set<VaccineProtocol>().Update(protocol);
}
