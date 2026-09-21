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

public sealed class ListFinancialTitlesQueryHandler : IRequestHandler<ListFinancialTitlesQuery, Result<IReadOnlyList<FinancialTitleDto>>>
{
    private readonly IFinancialTitleRepository _titleRepository;

    public ListFinancialTitlesQueryHandler(IFinancialTitleRepository titleRepository)
    {
        _titleRepository = titleRepository;
    }

    public async Task<Result<IReadOnlyList<FinancialTitleDto>>> Handle(ListFinancialTitlesQuery request, CancellationToken cancellationToken)
    {
        var titles = await _titleRepository.ListAsync(
            request.Direction,
            request.Status,
            request.PartyKind,
            request.PartyId,
            request.DueFrom,
            request.DueTo,
            cancellationToken);

        return Result.Success<IReadOnlyList<FinancialTitleDto>>(titles.Select(FinancialTitleMapper.ToDto).ToList());
    }
}
