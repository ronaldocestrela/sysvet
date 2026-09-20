using Microsoft.EntityFrameworkCore;
using Sales.Domain.Entities;
using Sales.Domain.Enums;
using Sales.Domain.Repositories;

namespace Sales.Infrastructure.Persistence.Repositories;

public sealed class PrepaidBalanceRepository : IPrepaidBalanceRepository
{
    private readonly SalesDbContext _dbContext;

    public PrepaidBalanceRepository(SalesDbContext dbContext) => _dbContext = dbContext;

    public async Task<PrepaidBalance?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.PrepaidBalances
            .Include(b => b.Credits)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public async Task<PrepaidBalance?> GetByPetAndServiceAsync(Guid petId, ServiceCode serviceCode, CancellationToken cancellationToken = default)
        => await _dbContext.PrepaidBalances
            .FirstOrDefaultAsync(b => b.PetId == petId && b.ServiceCode == serviceCode, cancellationToken);

    public async Task<PrepaidBalance?> GetByIdForCreditAsync(Guid petId, ServiceCode serviceCode, CancellationToken cancellationToken = default)
        => await _dbContext.PrepaidBalances
            .Include(b => b.Credits)
            .FirstOrDefaultAsync(b => b.PetId == petId && b.ServiceCode == serviceCode, cancellationToken);

    public async Task<IReadOnlyList<PrepaidBalance>> ListAsync(
        Guid? tutorId,
        Guid? petId,
        ServiceCode? serviceCode,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.PrepaidBalances.AsNoTracking().AsQueryable();
        if (tutorId is Guid t && t != Guid.Empty)
        {
            query = query.Where(b => b.TutorId == t);
        }

        if (petId is Guid p && p != Guid.Empty)
        {
            query = query.Where(b => b.PetId == p);
        }

        if (serviceCode.HasValue)
        {
            query = query.Where(b => b.ServiceCode == serviceCode.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public void Add(PrepaidBalance balance) => _dbContext.PrepaidBalances.Add(balance);

    public void Update(PrepaidBalance balance) => _dbContext.PrepaidBalances.Update(balance);
}
