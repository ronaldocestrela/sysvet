using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Domain;
using Inventory.Domain.Entities;

namespace Inventory.Domain.Repositories;

public interface IStockMovementRepository : IRepository<StockMovement>
{
    // Specific read operations if needed
}
