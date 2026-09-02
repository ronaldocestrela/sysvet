using Core.Domain;
using Sales.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sales.Domain.Repositories;

public interface IOrderRepository : IRepository<Order>
{
}
