using Core.Application.Entitlements;
using Core.Domain;
using MediatR;
using Platform.Application.Abstractions;
using Platform.Domain.Entities;
using PlatformErrorCodes = Platform.Domain.ErrorCodes;
using Platform.Domain.Repositories;
using Platform.Domain.Services;

namespace Platform.Application.Subscriptions;

/// <summary>Subscription mutation handlers (9.3).</summary>
public sealed class SubscriptionCommandHandlers :
    IRequestHandler<ChangeTenantPlanCommand, Result<PlanChangeResultDto>>,
    IRequestHandler<ActivateTenantAddOnCommand, Result<decimal>>,
    IRequestHandler<DeactivateTenantAddOnCommand, Result<decimal>>,
    IRequestHandler<SetTenantFeatureFlagCommand, Result>,
    IRequestHandler<ExpireDueTrialsCommand, Result<int>>
{
    private readonly IPlanRepository _planRepository;
    private readonly IAddOnRepository _addOnRepository;
    private readonly ITenantSubscriptionRepository _subscriptionRepository;
    private readonly IFeatureFlagRepository _featureFlagRepository;
    private readonly ISubscriptionAdjustmentRepository _adjustmentRepository;
    private readonly IPlatformUnitOfWork _unitOfWork;
    private readonly ITenantEntitlementReader _entitlementReader;

    /// <summary>Creates handlers.</summary>
    public SubscriptionCommandHandlers(
        IPlanRepository planRepository,
        IAddOnRepository addOnRepository,
        ITenantSubscriptionRepository subscriptionRepository,
        IFeatureFlagRepository featureFlagRepository,
        ISubscriptionAdjustmentRepository adjustmentRepository,
        IPlatformUnitOfWork unitOfWork,
        ITenantEntitlementReader entitlementReader)
    {
        _planRepository = planRepository;
        _addOnRepository = addOnRepository;
        _subscriptionRepository = subscriptionRepository;
        _featureFlagRepository = featureFlagRepository;
        _adjustmentRepository = adjustmentRepository;
        _unitOfWork = unitOfWork;
        _entitlementReader = entitlementReader;
    }

    /// <inheritdoc />
    public async Task<Result<PlanChangeResultDto>> Handle(ChangeTenantPlanCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetByTenantIdAsync(request.TenantId, cancellationToken);
        if (subscription?.Plan is null)
        {
            return Result.Failure<PlanChangeResultDto>(PlatformErrorCodes.Subscription.NotFound);
        }

        var newPlan = await _planRepository.GetByCodeAsync(request.PlanCode, cancellationToken);
        if (newPlan is null)
        {
            return Result.Failure<PlanChangeResultDto>(PlatformErrorCodes.Catalog.NotFound);
        }

        if (newPlan.Id == subscription.PlanId)
        {
            return Result.Success(new PlanChangeResultDto(0m, newPlan.Id));
        }

        var asOf = DateTimeOffset.UtcNow;
        var fromPlanId = subscription.PlanId;
        var proration = subscription.ChangePlan(newPlan.Id, subscription.Plan.MonthlyPrice, newPlan.MonthlyPrice, asOf);
        if (proration.IsFailure)
        {
            return Result.Failure<PlanChangeResultDto>(proration.Error);
        }

        var adjustment = SubscriptionAdjustment.CreatePending(
            request.TenantId,
            proration.Value,
            fromPlanId,
            newPlan.Id);
        if (adjustment.IsFailure)
        {
            return Result.Failure<PlanChangeResultDto>(adjustment.Error);
        }

        await _adjustmentRepository.AddAsync(adjustment.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _entitlementReader.Invalidate(request.TenantId);

        return Result.Success(new PlanChangeResultDto(proration.Value, newPlan.Id));
    }

    /// <inheritdoc />
    public async Task<Result<decimal>> Handle(ActivateTenantAddOnCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetByTenantIdAsync(request.TenantId, cancellationToken);
        if (subscription?.Plan is null)
        {
            return Result.Failure<decimal>(PlatformErrorCodes.Subscription.NotFound);
        }

        var addOn = await _addOnRepository.GetByCodeAsync(request.AddOnCode, cancellationToken);
        if (addOn is null)
        {
            return Result.Failure<decimal>(PlatformErrorCodes.Catalog.NotFound);
        }

        if (subscription.AddOns.Any(a => a.AddOnId == addOn.Id))
        {
            return Result.Failure<decimal>(PlatformErrorCodes.Subscription.AddOnAlreadyActive);
        }

        var asOf = DateTimeOffset.UtcNow;
        var daysRemaining = ProrationCalculator.DaysRemaining(subscription.PeriodStart, subscription.PeriodEnd, asOf);
        var delta = ProrationCalculator.CalculateDelta(0m, addOn.MonthlyPrice, daysRemaining);
        var charge = subscription.ApplyAddOnPriceDelta(delta);
        if (charge.IsFailure)
        {
            return charge;
        }

        subscription.AddOns.Add(new TenantAddOn(subscription.Id, addOn.Id, asOf));
        var adjustment = SubscriptionAdjustment.CreatePending(request.TenantId, charge.Value, null, null, addOn.Id);
        if (adjustment.IsSuccess && charge.Value != 0)
        {
            await _adjustmentRepository.AddAsync(adjustment.Value, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _entitlementReader.Invalidate(request.TenantId);
        return Result.Success(charge.Value);
    }

    /// <inheritdoc />
    public async Task<Result<decimal>> Handle(DeactivateTenantAddOnCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetByTenantIdAsync(request.TenantId, cancellationToken);
        if (subscription?.Plan is null)
        {
            return Result.Failure<decimal>(PlatformErrorCodes.Subscription.NotFound);
        }

        var addOn = await _addOnRepository.GetByCodeAsync(request.AddOnCode, cancellationToken);
        if (addOn is null)
        {
            return Result.Failure<decimal>(PlatformErrorCodes.Catalog.NotFound);
        }

        var link = subscription.AddOns.FirstOrDefault(a => a.AddOnId == addOn.Id);
        if (link is null)
        {
            return Result.Failure<decimal>(PlatformErrorCodes.Subscription.AddOnNotActive);
        }

        var asOf = DateTimeOffset.UtcNow;
        var daysRemaining = ProrationCalculator.DaysRemaining(subscription.PeriodStart, subscription.PeriodEnd, asOf);
        var delta = ProrationCalculator.CalculateDelta(addOn.MonthlyPrice, 0m, daysRemaining);
        var charge = subscription.ApplyAddOnPriceDelta(delta);
        if (charge.IsFailure)
        {
            return charge;
        }

        subscription.AddOns.Remove(link);
        var adjustment = SubscriptionAdjustment.CreatePending(request.TenantId, charge.Value, null, null, addOn.Id);
        if (adjustment.IsSuccess && charge.Value != 0)
        {
            await _adjustmentRepository.AddAsync(adjustment.Value, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _entitlementReader.Invalidate(request.TenantId);
        return Result.Success(charge.Value);
    }

    /// <inheritdoc />
    public async Task<Result> Handle(SetTenantFeatureFlagCommand request, CancellationToken cancellationToken)
    {
        var flagResult = FeatureFlag.Create(request.TenantId, request.Module, request.State);
        if (flagResult.IsFailure)
        {
            return Result.Failure(flagResult.Error);
        }

        await _featureFlagRepository.UpsertAsync(flagResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _entitlementReader.Invalidate(request.TenantId);
        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result<int>> Handle(ExpireDueTrialsCommand request, CancellationToken cancellationToken)
    {
        var due = await _subscriptionRepository.ListDueTrialsAsync(request.AsOfUtc, cancellationToken);
        var count = 0;
        foreach (var subscription in due)
        {
            var amount = subscription.ExpireTrial(request.AsOfUtc);
            if (amount.IsFailure)
            {
                continue;
            }

            if (subscription.TrialEndAction == TrialEndAction.Convert)
            {
                var adjustment = SubscriptionAdjustment.CreatePending(subscription.TenantId, 0m, subscription.PlanId, subscription.PlanId);
                if (adjustment.IsSuccess)
                {
                    await _adjustmentRepository.AddAsync(adjustment.Value, cancellationToken);
                }
            }

            _entitlementReader.Invalidate(subscription.TenantId);
            count++;
        }

        if (count > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Success(count);
    }
}
