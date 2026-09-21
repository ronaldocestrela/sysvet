namespace Fiscal.Domain.Repositories;

/// <summary>Unit of work for the Fiscal module.</summary>
public interface IFiscalUnitOfWork
{
    /// <summary>Persists pending changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
