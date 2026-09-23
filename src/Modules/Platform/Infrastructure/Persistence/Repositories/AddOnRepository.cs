using Microsoft.EntityFrameworkCore;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;

namespace Platform.Infrastructure.Persistence.Repositories;

/// <inheritdoc />
public sealed class AddOnRepository : IAddOnRepository
{
    private readonly PlatformDbContext _context;

    /// <summary>Creates the repository.</summary>
    public AddOnRepository(PlatformDbContext context) => _context = context;

    /// <inheritdoc />
    public Task<IReadOnlyList<AddOn>> ListAsync(CancellationToken cancellationToken = default) =>
        _context.AddOns.OrderBy(a => a.Code).ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<AddOn>)t.Result, cancellationToken);

    /// <inheritdoc />
    public Task<AddOn?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.AddOns.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    /// <inheritdoc />
    public Task<AddOn?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        _context.AddOns.FirstOrDefaultAsync(a => a.Code == code, cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(AddOn addOn, CancellationToken cancellationToken = default) =>
        await _context.AddOns.AddAsync(addOn, cancellationToken);

    /// <inheritdoc />
    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default) =>
        _context.AddOns.AnyAsync(a => a.Code == code, cancellationToken);
}
