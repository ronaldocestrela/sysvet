using Core.Domain;
using Platform.Application.Abstractions;
using Platform.Application.Provisioning;
using Platform.Domain.Catalog;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;

namespace Platform.Infrastructure.Provisioning;

/// <inheritdoc />
public sealed class TenantSubscriptionProvisioner : ITenantSubscriptionProvisioner
{
    private readonly IPlanRepository _planRepository;
    private readonly IAddOnRepository _addOnRepository;
    private readonly ITenantSubscriptionRepository _subscriptionRepository;
    private readonly IPlatformUnitOfWork _unitOfWork;

    /// <summary>Creates the provisioner.</summary>
    public TenantSubscriptionProvisioner(
        IPlanRepository planRepository,
        IAddOnRepository addOnRepository,
        ITenantSubscriptionRepository subscriptionRepository,
        IPlatformUnitOfWork unitOfWork)
    {
        _planRepository = planRepository;
        _addOnRepository = addOnRepository;
        _subscriptionRepository = subscriptionRepository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<Result> ProvisionNewTenantAsync(
        Guid tenantId,
        string? planCode,
        int? trialDays,
        TrialEndAction? trialEndAction,
        CancellationToken cancellationToken = default)
    {
        if (await _subscriptionRepository.GetByTenantIdAsync(tenantId, cancellationToken) is not null)
        {
            return Result.Success();
        }

        var code = string.IsNullOrWhiteSpace(planCode) ? CatalogCodes.Plans.Starter : planCode.Trim();
        var plan = await _planRepository.GetByCodeAsync(code, cancellationToken);
        if (plan is null)
        {
            return Result.Failure(Platform.Domain.ErrorCodes.Catalog.NotFound);
        }

        var now = DateTimeOffset.UtcNow;
        var endAction = trialEndAction ?? TrialEndAction.Block;
        var subscriptionResult = trialDays is > 0
            ? TenantSubscription.CreateTrial(tenantId, plan.Id, now, trialDays.Value, endAction)
            : TenantSubscription.CreateActive(tenantId, plan.Id, now);

        if (subscriptionResult.IsFailure)
        {
            return Result.Failure(subscriptionResult.Error);
        }

        await _subscriptionRepository.AddAsync(subscriptionResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result> ProvisionGrandfatherAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (await _subscriptionRepository.GetByTenantIdAsync(tenantId, cancellationToken) is not null)
        {
            return Result.Success();
        }

        var plan = await _planRepository.GetByCodeAsync(CatalogCodes.Plans.Hospital24h, cancellationToken);
        if (plan is null)
        {
            return Result.Failure(Platform.Domain.ErrorCodes.Catalog.NotFound);
        }

        var now = DateTimeOffset.UtcNow;
        var subscriptionResult = TenantSubscription.CreateActive(tenantId, plan.Id, now);
        if (subscriptionResult.IsFailure)
        {
            return Result.Failure(subscriptionResult.Error);
        }

        var subscription = subscriptionResult.Value;
        var addOns = await _addOnRepository.ListAsync(cancellationToken);
        foreach (var addOn in addOns)
        {
            subscription.AddOns.Add(new TenantAddOn(subscription.Id, addOn.Id, now));
        }

        await _subscriptionRepository.AddAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
