using Core.Domain;
using Finance.Application.Reconciliation.Dtos;
using Finance.Domain.Repositories;
using MediatR;

namespace Finance.Application.Reconciliation;

public sealed class ListCardReconciliationsQueryHandler
    : IRequestHandler<ListCardReconciliationsQuery, Result<IReadOnlyList<CardReconciliationBatchSummaryDto>>>
{
    private readonly ICardReconciliationRepository _repository;

    public ListCardReconciliationsQueryHandler(ICardReconciliationRepository repository) => _repository = repository;

    public async Task<Result<IReadOnlyList<CardReconciliationBatchSummaryDto>>> Handle(
        ListCardReconciliationsQuery request,
        CancellationToken cancellationToken)
    {
        var batches = await _repository.ListAsync(cancellationToken);
        var summaries = batches
            .Select(b => CardReconciliationMapper.ToSummary(b))
            .ToList();
        return Result.Success<IReadOnlyList<CardReconciliationBatchSummaryDto>>(summaries);
    }
}

public sealed class GetCardReconciliationByIdQueryHandler
    : IRequestHandler<GetCardReconciliationByIdQuery, Result<CardReconciliationBatchDetailDto?>>
{
    private readonly ICardReconciliationRepository _repository;

    public GetCardReconciliationByIdQueryHandler(ICardReconciliationRepository repository) => _repository = repository;

    public async Task<Result<CardReconciliationBatchDetailDto?>> Handle(
        GetCardReconciliationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var batch = await _repository.GetByIdAsync(request.BatchId, cancellationToken);
        if (batch is null)
        {
            return Result.Success<CardReconciliationBatchDetailDto?>(null);
        }

        return Result.Success<CardReconciliationBatchDetailDto?>(CardReconciliationMapper.ToDetail(batch));
    }
}

public sealed class GetUnmatchedCardSettlementsQueryHandler
    : IRequestHandler<GetUnmatchedCardSettlementsQuery, Result<IReadOnlyList<UnmatchedCardSettlementDto>>>
{
    private readonly ICardReconciliationRepository _reconciliationRepository;
    private readonly IFinancialTitleRepository _titleRepository;

    public GetUnmatchedCardSettlementsQueryHandler(
        ICardReconciliationRepository reconciliationRepository,
        IFinancialTitleRepository titleRepository)
    {
        _reconciliationRepository = reconciliationRepository;
        _titleRepository = titleRepository;
    }

    public async Task<Result<IReadOnlyList<UnmatchedCardSettlementDto>>> Handle(
        GetUnmatchedCardSettlementsQuery request,
        CancellationToken cancellationToken)
    {
        var matchedIds = (await _reconciliationRepository.ListMatchedAllocationIdsAsync(cancellationToken)).ToHashSet();
        var snapshots = await _titleRepository.ListCardSettlementSnapshotsAsync(cancellationToken);

        var unmatched = snapshots
            .Where(s => !matchedIds.Contains(s.AllocationId))
            .Select(s => new UnmatchedCardSettlementDto
            {
                AllocationId = s.AllocationId,
                Nsu = s.Nsu,
                Amount = s.Amount,
                Method = s.Method
            })
            .ToList();

        return Result.Success<IReadOnlyList<UnmatchedCardSettlementDto>>(unmatched);
    }
}
