using Core.Domain;
using Sales.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sales.Domain.Repositories;

public interface ICashRegisterRepository : IRepository<CashRegister>
{
    Task<CashRegister?> GetOpenCashRegisterByUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
