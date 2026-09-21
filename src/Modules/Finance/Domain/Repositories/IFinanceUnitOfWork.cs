using Core.Domain;

namespace Finance.Domain.Repositories;

/// <summary>
/// Unit of work boundary for the Finance module.
/// </summary>
public interface IFinanceUnitOfWork : IChangeTrackingUnitOfWork;
