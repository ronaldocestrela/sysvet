using Core.Application.Messaging;
using Core.Domain;
using MediatR;
using Finance.Domain.Entities;
using Finance.Domain.Repositories;

namespace Finance.Application.Titles.Commands;

public sealed class CreateManualFinancialTitleCommandHandler : IRequestHandler<CreateManualFinancialTitleCommand, Result<Guid>>
{
    private readonly IFinancialTitleRepository _titleRepository;
    private readonly IFinancialCategoryRepository _categoryRepository;
    private readonly ICostCenterRepository _costCenterRepository;

    public CreateManualFinancialTitleCommandHandler(
        IFinancialTitleRepository titleRepository,
        IFinancialCategoryRepository categoryRepository,
        ICostCenterRepository costCenterRepository)
    {
        _titleRepository = titleRepository;
        _categoryRepository = categoryRepository;
        _costCenterRepository = costCenterRepository;
    }

    public async Task<Result<Guid>> Handle(CreateManualFinancialTitleCommand request, CancellationToken cancellationToken)
    {
        if (await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken) is null)
        {
            return Result.Failure<Guid>(Finance.Domain.ErrorCodes.Category.NotFound);
        }

        if (request.CostCenterId is { } ccId && await _costCenterRepository.GetByIdAsync(ccId, cancellationToken) is null)
        {
            return Result.Failure<Guid>(Finance.Domain.ErrorCodes.CostCenter.NotFound);
        }

        var created = FinancialTitle.CreateManual(
            request.Direction,
            request.CategoryId,
            request.PartyKind,
            request.PartyId,
            request.Amount,
            request.IssueDate,
            request.DueDate,
            request.Description,
            request.CostCenterId,
            request.Id == Guid.Empty ? null : request.Id);

        if (created.IsFailure)
        {
            return Result.Failure<Guid>(created.Error);
        }

        _titleRepository.Add(created.Value);
        return Result.Success(created.Value.Id);
    }
}

public sealed class SettleFinancialTitleCommandHandler : IRequestHandler<SettleFinancialTitleCommand, Result>
{
    private readonly IFinancialTitleRepository _titleRepository;

    public SettleFinancialTitleCommandHandler(IFinancialTitleRepository titleRepository)
    {
        _titleRepository = titleRepository;
    }

    public async Task<Result> Handle(SettleFinancialTitleCommand request, CancellationToken cancellationToken)
    {
        var title = await _titleRepository.GetByIdAsync(request.TitleId, cancellationToken);
        if (title is null)
        {
            return Result.Failure(Finance.Domain.ErrorCodes.Title.NotFound);
        }

        var correlation = request.IdempotencyKey == Guid.Empty ? Guid.NewGuid() : request.IdempotencyKey;
        var paidAt = request.PaidAt ?? DateTimeOffset.UtcNow;
        var result = title.Allocate(request.Amount, paidAt, request.Method, correlation);
        if (result.IsFailure)
        {
            return result;
        }

        _titleRepository.Update(title);
        return Result.Success();
    }
}

public sealed class CancelFinancialTitleCommandHandler : IRequestHandler<CancelFinancialTitleCommand, Result>
{
    private readonly IFinancialTitleRepository _titleRepository;

    public CancelFinancialTitleCommandHandler(IFinancialTitleRepository titleRepository)
    {
        _titleRepository = titleRepository;
    }

    public async Task<Result> Handle(CancelFinancialTitleCommand request, CancellationToken cancellationToken)
    {
        var title = await _titleRepository.GetByIdAsync(request.TitleId, cancellationToken);
        if (title is null)
        {
            return Result.Failure(Finance.Domain.ErrorCodes.Title.NotFound);
        }

        var result = title.Cancel();
        if (result.IsFailure)
        {
            return result;
        }

        _titleRepository.Update(title);
        return Result.Success();
    }
}
