using Microsoft.EntityFrameworkCore;
using Petshop.Domain.Entities;
using Petshop.Domain.Repositories;

namespace Petshop.Infrastructure.Persistence.Repositories;

public sealed class GroomingRecordRepository : IGroomingRecordRepository
{
    private readonly PetshopDbContext _dbContext;

    public GroomingRecordRepository(PetshopDbContext dbContext) => _dbContext = dbContext;

    public void Add(GroomingRecord entity) => _dbContext.GroomingRecords.Add(entity);

    public async Task AddAsync(GroomingRecord entity, CancellationToken cancellationToken = default)
        => await _dbContext.GroomingRecords.AddAsync(entity, cancellationToken);

    public async Task<IEnumerable<GroomingRecord>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.GroomingRecords.ToListAsync(cancellationToken);

    public async Task<GroomingRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.GroomingRecords
            .Include(r => r.SupplyLines)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<GroomingRecord?> GetByAppointmentIdAsync(Guid groomingAppointmentId, CancellationToken cancellationToken = default)
        => await _dbContext.GroomingRecords
            .Include(r => r.SupplyLines)
            .FirstOrDefaultAsync(r => r.GroomingAppointmentId == groomingAppointmentId, cancellationToken);

    public async Task<IReadOnlyList<GroomingRecord>> ListByPetIdAsync(Guid petId, CancellationToken cancellationToken = default)
        => await _dbContext.GroomingRecords
            .Include(r => r.SupplyLines)
            .Where(r => r.PetId == petId)
            .ToListAsync(cancellationToken);

    public void Remove(GroomingRecord entity) => _dbContext.GroomingRecords.Remove(entity);

    public void Update(GroomingRecord entity) => _dbContext.GroomingRecords.Update(entity);
}
