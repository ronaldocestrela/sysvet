using Core.Application.Messaging;
using Core.Domain;
using MediatR;
using Finance.Application.Projections.Dtos;
using Finance.Domain.Enums;
using Finance.Domain.Repositories;

namespace Finance.Application.Projections;

public sealed class GetBalanceProjectionQueryHandler : IRequestHandler<GetBalanceProjectionQuery, Result<BalanceProjectionDto>>
{
    private readonly IFinancialTitleRepository _titleRepository;

    public GetBalanceProjectionQueryHandler(IFinancialTitleRepository titleRepository)
    {
        _titleRepository = titleRepository;
    }

    public async Task<Result<BalanceProjectionDto>> Handle(GetBalanceProjectionQuery request, CancellationToken cancellationToken)
    {
        var titles = await _titleRepository.ListForStatementsAsync(request.From, request.To, cancellationToken);

        decimal expectedReceivable = 0;
        decimal expectedPayable = 0;
        decimal realizedReceivable = 0;
        decimal realizedPayable = 0;

        foreach (var title in titles.Where(t => t.Status != TitleStatus.Cancelled))
        {
            if (title.DueDate >= request.From && title.DueDate <= request.To)
            {
                if (title.Direction == TitleDirection.Receivable)
                {
                    expectedReceivable += title.OpenAmount;
                }
                else
                {
                    expectedPayable += title.OpenAmount;
                }
            }

            var netRealized = NetRealizedInPeriod(title, request.From, request.To);
            if (title.Direction == TitleDirection.Receivable)
            {
                realizedReceivable += netRealized;
            }
            else
            {
                realizedPayable += netRealized;
            }
        }

        return Result.Success(new BalanceProjectionDto
        {
            From = request.From,
            To = request.To,
            ExpectedReceivable = expectedReceivable,
            ExpectedPayable = expectedPayable,
            RealizedReceivable = realizedReceivable,
            RealizedPayable = realizedPayable
        });
    }

    private static decimal NetRealizedInPeriod(Finance.Domain.Entities.FinancialTitle title, DateOnly from, DateOnly to)
    {
        decimal total = 0;
        foreach (var allocation in title.Allocations)
        {
            var paidDate = DateOnly.FromDateTime(allocation.PaidAt.UtcDateTime);
            if (paidDate < from || paidDate > to)
            {
                continue;
            }

            total += allocation.Kind == AllocationKind.Settlement ? allocation.Amount : -allocation.Amount;
        }

        return total;
    }
}

public sealed class GetPartyLedgerQueryHandler : IRequestHandler<GetPartyLedgerQuery, Result<IReadOnlyList<PartyLedgerEntryDto>>>
{
    private readonly IFinancialTitleRepository _titleRepository;

    public GetPartyLedgerQueryHandler(IFinancialTitleRepository titleRepository)
    {
        _titleRepository = titleRepository;
    }

    public async Task<Result<IReadOnlyList<PartyLedgerEntryDto>>> Handle(GetPartyLedgerQuery request, CancellationToken cancellationToken)
    {
        var titles = await _titleRepository.ListAsync(null, null, request.PartyKind, request.PartyId, null, null, cancellationToken);
        var entries = titles
            .OrderByDescending(t => t.DueDate)
            .Select(t => new PartyLedgerEntryDto
            {
                TitleId = t.Id,
                Description = t.Description,
                DueDate = t.DueDate,
                OriginalAmount = t.OriginalAmount,
                OpenAmount = t.OpenAmount,
                Status = t.Status.ToString()
            })
            .ToList();

        return Result.Success<IReadOnlyList<PartyLedgerEntryDto>>(entries);
    }
}
