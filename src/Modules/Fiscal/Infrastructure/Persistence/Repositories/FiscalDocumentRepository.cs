using Fiscal.Domain.Entities;
using Fiscal.Domain.Enums;
using Fiscal.Domain.Repositories;
using Fiscal.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace Fiscal.Infrastructure.Persistence.Repositories;

/// <summary>EF fiscal document repository.</summary>
public sealed class FiscalDocumentRepository : IFiscalDocumentRepository
{
    private readonly FiscalDbContext _dbContext;

    public FiscalDocumentRepository(FiscalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<FiscalDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.FiscalDocuments
            .Include(d => d.Items)
            .Include(d => d.Corrections)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public Task<FiscalDocument?> GetAuthorizedByOrderAsync(
        Guid orderId,
        FiscalDocumentType type,
        CancellationToken cancellationToken = default) =>
        _dbContext.FiscalDocuments
            .FirstOrDefaultAsync(
                d => d.SourceOrderId == orderId
                     && d.DocumentType == type
                     && d.Status == FiscalDocumentStatus.Authorized,
                cancellationToken);

    public async Task<FiscalDocument?> GetByOrderAndTypeAsync(
        Guid orderId,
        FiscalDocumentType type,
        CancellationToken cancellationToken = default)
    {
        var matches = await _dbContext.FiscalDocuments
            .Include(d => d.Items)
            .Where(d => d.SourceOrderId == orderId
                        && d.DocumentType == type
                        && d.Status != FiscalDocumentStatus.Cancelled)
            .ToListAsync(cancellationToken);

        return matches.OrderByDescending(d => d.UpdatedAt).FirstOrDefault();
    }

    public async Task<IReadOnlyList<FiscalDocument>> ListAsync(
        Guid? orderId,
        FiscalDocumentStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.FiscalDocuments.AsQueryable();
        if (orderId.HasValue)
        {
            query = query.Where(d => d.SourceOrderId == orderId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(d => d.Status == status.Value);
        }

        var list = await query.ToListAsync(cancellationToken);
        return list.OrderByDescending(d => d.UpdatedAt).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<FiscalDocument>> ListForPlanningAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        var rbt12Start = FiscalPlanningCalculator.GetRbt12Start(to);

        var list = await _dbContext.FiscalDocuments
            .Include(d => d.Items)
            .AsNoTracking()
            .Where(d => d.Status == FiscalDocumentStatus.Authorized || d.Status == FiscalDocumentStatus.Cancelled)
            .ToListAsync(cancellationToken);

        return list
            .Where(d => IsInPlanningWindow(d, from, to, rbt12Start))
            .ToList();
    }

    public void Add(FiscalDocument document) => _dbContext.FiscalDocuments.Add(document);

    public void Update(FiscalDocument document) => _dbContext.FiscalDocuments.Update(document);

    private static bool IsInPlanningWindow(FiscalDocument document, DateOnly from, DateOnly to, DateOnly rbt12Start)
    {
        if (document.AuthorizedAt is not null)
        {
            var authorized = DateOnly.FromDateTime(document.AuthorizedAt.Value.UtcDateTime);
            if (authorized >= rbt12Start && authorized <= to)
            {
                return true;
            }
        }

        if (document.CancelledAt is not null)
        {
            var cancelled = DateOnly.FromDateTime(document.CancelledAt.Value.UtcDateTime);
            if (cancelled >= from && cancelled <= to)
            {
                return true;
            }
        }

        return false;
    }
}
