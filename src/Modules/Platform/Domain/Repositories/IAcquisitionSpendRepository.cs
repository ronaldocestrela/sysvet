using Platform.Domain.Entities;

namespace Platform.Domain.Repositories;

/// <summary>Acquisition spend persistence for CAC (10.2).</summary>
public interface IAcquisitionSpendRepository
{
    /// <summary>Gets spend row for month and channel.</summary>
    Task<AcquisitionSpend?> GetByMonthAndChannelAsync(int year, int month, string channel, CancellationToken cancellationToken = default);

    /// <summary>Sums all spend rows for a civil month.</summary>
    Task<decimal> SumByMonthAsync(int year, int month, CancellationToken cancellationToken = default);

    /// <summary>Persists a new spend row.</summary>
    Task AddAsync(AcquisitionSpend spend, CancellationToken cancellationToken = default);
}
