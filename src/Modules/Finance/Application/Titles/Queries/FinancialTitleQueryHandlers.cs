using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain;
using MediatR;
using Finance.Application.Titles.Dtos;
using Finance.Domain.Repositories;

namespace Finance.Application.Titles.Queries;

public sealed class GetFinancialTitleByIdQueryHandler : IRequestHandler<GetFinancialTitleByIdQuery, Result<FinancialTitleDto>>
{
    private readonly IFinancialTitleRepository _titleRepository;

    public GetFinancialTitleByIdQueryHandler(IFinancialTitleRepository titleRepository)
    {
        _titleRepository = titleRepository;
    }

    public async Task<Result<FinancialTitleDto>> Handle(GetFinancialTitleByIdQuery request, CancellationToken cancellationToken)
    {
        var title = await _titleRepository.GetByIdAsync(request.Id, cancellationToken);
        if (title is null)
        {
            return Result.Failure<FinancialTitleDto>(Finance.Domain.ErrorCodes.Title.NotFound);
        }

        return Result.Success(FinancialTitleMapper.ToDto(title));
    }
}

public sealed class ListFinancialTitlesQueryHandler : IRequestHandler<ListFinancialTitlesQuery, Result<PagedResult<FinancialTitleDto>>>
{
    private readonly IFinancialTitleRepository _titleRepository;

    public ListFinancialTitlesQueryHandler(IFinancialTitleRepository titleRepository)
    {
        _titleRepository = titleRepository;
    }

    public async Task<Result<PagedResult<FinancialTitleDto>>> Handle(ListFinancialTitlesQuery request, CancellationToken cancellationToken)
    {
        var pageRequest = PageRequest.TryCreate(request.Page, request.PageSize);
        if (pageRequest.IsFailure)
        {
            return Result.Failure<PagedResult<FinancialTitleDto>>(pageRequest.Error);
        }

        var (page, pageSize) = (pageRequest.Value.Page, pageRequest.Value.PageSize);
        var (titles, total) = await _titleRepository.ListPagedAsync(
            request.Direction,
            request.Status,
            request.PartyKind,
            request.PartyId,
            request.DueFrom,
            request.DueTo,
            page,
            pageSize,
            cancellationToken);

        var items = titles.Select(FinancialTitleMapper.ToDto).ToList();
        return Result.Success(new PagedResult<FinancialTitleDto>(items, page, pageSize, total));
    }
}
