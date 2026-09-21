using Core.Domain;
using Finance.Domain.Entities;
using Finance.Domain.Models;
using Finance.Domain.Repositories;
using MediatR;

namespace Finance.Application.Reconciliation;

public sealed class ImportCardStatementCommandHandler : IRequestHandler<ImportCardStatementCommand, Result<Guid>>
{
    private readonly ICardReconciliationRepository _reconciliationRepository;
    private readonly IFinancialTitleRepository _titleRepository;

    public ImportCardStatementCommandHandler(
        ICardReconciliationRepository reconciliationRepository,
        IFinancialTitleRepository titleRepository)
    {
        _reconciliationRepository = reconciliationRepository;
        _titleRepository = titleRepository;
    }

    public async Task<Result<Guid>> Handle(ImportCardStatementCommand request, CancellationToken cancellationToken)
    {
        var statementLines = request.Lines
            .Select(l => new CardStatementImportLine(
                l.Nsu,
                l.Amount,
                l.Method,
                l.Fee,
                l.OccurredAt ?? DateTimeOffset.UtcNow))
            .ToList();

        var settlements = await _titleRepository.ListCardSettlementSnapshotsAsync(cancellationToken);

        var batchResult = CardReconciliationBatch.Import(
            request.Reference,
            request.PeriodFrom,
            request.PeriodTo,
            statementLines,
            settlements);

        if (batchResult.IsFailure)
        {
            return Result.Failure<Guid>(batchResult.Error);
        }

        _reconciliationRepository.Add(batchResult.Value);
        return Result.Success(batchResult.Value.Id);
    }
}
