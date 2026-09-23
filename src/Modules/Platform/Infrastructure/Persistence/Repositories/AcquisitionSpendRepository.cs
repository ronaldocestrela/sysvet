using Microsoft.EntityFrameworkCore;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;

namespace Platform.Infrastructure.Persistence.Repositories;

/// <inheritdoc />
public sealed class AcquisitionSpendRepository : IAcquisitionSpendRepository
{
    private readonly PlatformDbContext _context;

    /// <summary>Creates the repository.</summary>
    public AcquisitionSpendRepository(PlatformDbContext context) => _context = context;

    /// <inheritdoc />
    public Task<AcquisitionSpend?> GetByMonthAndChannelAsync(int year, int month, string channel, CancellationToken cancellationToken = default) =>
        _context.AcquisitionSpends.FirstOrDefaultAsync(
            s => s.Year == year && s.Month == month && s.Channel == channel,
            cancellationToken);

    /// <inheritdoc />
    public Task<decimal> SumByMonthAsync(int year, int month, CancellationToken cancellationToken = default) =>
        _context.AcquisitionSpends
            .AsNoTracking()
            .Where(s => s.Year == year && s.Month == month)
            .SumAsync(s => s.Amount, cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(AcquisitionSpend spend, CancellationToken cancellationToken = default) =>
        await _context.AcquisitionSpends.AddAsync(spend, cancellationToken);
}
