using Finance.Domain.Entities;
using Finance.Domain.Enums;
using Finance.Domain.Models;

namespace Finance.Domain.Repositories;

/// <summary>
/// Persistence for financial titles and allocations.
/// </summary>
public interface IFinancialTitleRepository
{
    Task<FinancialTitle?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<FinancialTitle?> GetBySourceAsync(
        TitleSourceType sourceType,
        Guid sourceId,
        string installmentKey,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<FinancialTitle>> ListBySourceIdAsync(
        TitleSourceType sourceType,
        Guid sourceId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<FinancialTitle>> ListAsync(
        TitleDirection? direction,
        TitleStatus? status,
        PartyKind? partyKind,
        Guid? partyId,
        DateOnly? dueFrom,
        DateOnly? dueTo,
        CancellationToken cancellationToken);

    /// <summary>
    /// Loads titles relevant to statements: issue date, due date, or any allocation paid in the inclusive range.
    /// </summary>
    Task<IReadOnlyList<FinancialTitle>> ListForStatementsAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<CardSettlementSnapshot>> ListCardSettlementSnapshotsAsync(CancellationToken cancellationToken);

    void Add(FinancialTitle title);

    void Update(FinancialTitle title);
}
