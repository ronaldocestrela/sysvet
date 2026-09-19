using Core.Domain;
using MediatR;
using Sales.Domain.Entities;
using Sales.Domain.Repositories;

namespace Sales.Application.Commissions;

public sealed class UpsertCommissionRuleCommandHandler : IRequestHandler<UpsertCommissionRuleCommand, Result<Guid>>
{
    private readonly ICommissionRuleRepository _repository;
    private readonly ISalesUnitOfWork _unitOfWork;

    public UpsertCommissionRuleCommandHandler(ICommissionRuleRepository repository, ISalesUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(UpsertCommissionRuleCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByRoleAndAppliesToAsync(request.Role, request.AppliesTo, cancellationToken);
        if (existing is null)
        {
            var created = CommissionRule.Create(request.Role, request.AppliesTo, request.RatePercent);
            if (created.IsFailure)
            {
                return Result.Failure<Guid>(created.Error);
            }

            _repository.Add(created.Value);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(created.Value.Id);
        }

        var update = existing.SetRate(request.RatePercent);
        if (update.IsFailure)
        {
            return Result.Failure<Guid>(update.Error);
        }

        _repository.Update(existing);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(existing.Id);
    }
}

public sealed class ListCommissionRulesQueryHandler : IRequestHandler<ListCommissionRulesQuery, Result<IReadOnlyList<CommissionRuleDto>>>
{
    private readonly ICommissionRuleRepository _repository;

    public ListCommissionRulesQueryHandler(ICommissionRuleRepository repository) => _repository = repository;

    public async Task<Result<IReadOnlyList<CommissionRuleDto>>> Handle(ListCommissionRulesQuery request, CancellationToken cancellationToken)
    {
        var rules = await _repository.ListAllAsync(cancellationToken);
        var dtos = rules
            .Select(r => new CommissionRuleDto(r.Id, r.Role, r.AppliesTo, r.RatePercent))
            .ToList();
        return Result.Success<IReadOnlyList<CommissionRuleDto>>(dtos);
    }
}

public sealed class ListCommissionAccrualsQueryHandler : IRequestHandler<ListCommissionAccrualsQuery, Result<IReadOnlyList<CommissionAccrualDto>>>
{
    private readonly IOrderRepository _orderRepository;

    public ListCommissionAccrualsQueryHandler(IOrderRepository orderRepository) => _orderRepository = orderRepository;

    public async Task<Result<IReadOnlyList<CommissionAccrualDto>>> Handle(ListCommissionAccrualsQuery request, CancellationToken cancellationToken)
    {
        if (request.OrderId is not Guid orderId)
        {
            return Result.Success<IReadOnlyList<CommissionAccrualDto>>(Array.Empty<CommissionAccrualDto>());
        }

        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure<IReadOnlyList<CommissionAccrualDto>>(Sales.Domain.ErrorCodes.Order.NotFound);
        }

        var dtos = order.Commissions
            .Select(c => new CommissionAccrualDto(
                c.Id,
                c.OrderId,
                c.OrderItemId,
                c.PayeeUserId,
                c.Role,
                c.RatePercent,
                c.BaseAmount.Amount,
                c.CommissionAmount.Amount,
                c.Status))
            .ToList();

        return Result.Success<IReadOnlyList<CommissionAccrualDto>>(dtos);
    }
}
