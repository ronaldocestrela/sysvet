using Microsoft.EntityFrameworkCore;
using Sales.Domain.Entities;
using Sales.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sales.Infrastructure.Persistence.Repositories;

public class CashRegisterRepository : ICashRegisterRepository
{
    private readonly SalesDbContext _dbContext;

    public CashRegisterRepository(SalesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CashRegister?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.CashRegisters
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<CashRegister?> GetOpenCashRegisterByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.CashRegisters
            .FirstOrDefaultAsync(c => c.OpenedByUserId == userId && c.Status == "Open", cancellationToken);
    }

    public async Task<IEnumerable<CashRegister>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.CashRegisters.ToListAsync(cancellationToken);
    }

    public void Add(CashRegister cashRegister)
    {
        _dbContext.CashRegisters.Add(cashRegister);
    }

    public void Update(CashRegister cashRegister)
    {
        _dbContext.CashRegisters.Update(cashRegister);
    }

    public void Remove(CashRegister cashRegister)
    {
        _dbContext.CashRegisters.Remove(cashRegister);
    }
}
