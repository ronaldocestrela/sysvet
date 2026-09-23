using Core.Application.Entitlements;
using Core.Domain;
using MediatR;
using Platform.Application.Billing;
using Platform.Domain.Repositories;
using PlatformErrorCodes = Platform.Domain.ErrorCodes;

namespace Platform.Application.Subscriptions;

/// <summary>Catalog and subscription query handlers (9.3).</summary>
public sealed class SubscriptionQueryHandlers :
    IRequestHandler<ListPlansQuery, Result<IReadOnlyList<PlanSummaryDto>>>,
    IRequestHandler<ListAddOnsQuery, Result<IReadOnlyList<AddOnSummaryDto>>>,
    IRequestHandler<GetTenantSubscriptionQuery, Result<TenantSubscriptionDto>>,
    IRequestHandler<GetTenantEntitlementsQuery, Result<TenantEntitlementsDto>>,
    IRequestHandler<ListTenantBillingInvoicesQuery, Result<IReadOnlyList<BillingInvoiceDto>>>
{
    private readonly IPlanRepository _planRepository;
    private readonly IAddOnRepository _addOnRepository;
    private readonly ITenantSubscriptionRepository _subscriptionRepository;
    private readonly ITenantEntitlementReader _entitlementReader;
    private readonly IBillingInvoiceRepository _billingInvoiceRepository;

    /// <summary>Creates handlers.</summary>
    public SubscriptionQueryHandlers(
        IPlanRepository planRepository,
        IAddOnRepository addOnRepository,
        ITenantSubscriptionRepository subscriptionRepository,
        ITenantEntitlementReader entitlementReader,
        IBillingInvoiceRepository billingInvoiceRepository)
    {
        _planRepository = planRepository;
        _addOnRepository = addOnRepository;
        _subscriptionRepository = subscriptionRepository;
        _entitlementReader = entitlementReader;
        _billingInvoiceRepository = billingInvoiceRepository;
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<PlanSummaryDto>>> Handle(ListPlansQuery request, CancellationToken cancellationToken)
    {
        var plans = await _planRepository.ListAsync(cancellationToken);
        var dtos = plans.Select(p => new PlanSummaryDto(p.Id, p.Code, p.Name, p.MonthlyPrice, p.GetModuleList())).ToList();
        return Result.Success<IReadOnlyList<PlanSummaryDto>>(dtos);
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<AddOnSummaryDto>>> Handle(ListAddOnsQuery request, CancellationToken cancellationToken)
    {
        var addOns = await _addOnRepository.ListAsync(cancellationToken);
        var dtos = addOns.Select(a => new AddOnSummaryDto(a.Id, a.Code, a.Name, a.MonthlyPrice, a.Module)).ToList();
        return Result.Success<IReadOnlyList<AddOnSummaryDto>>(dtos);
    }

    /// <inheritdoc />
    public async Task<Result<TenantSubscriptionDto>> Handle(GetTenantSubscriptionQuery request, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetByTenantIdAsync(request.TenantId, cancellationToken);
        if (subscription?.Plan is null)
        {
            return Result.Failure<TenantSubscriptionDto>(PlatformErrorCodes.Subscription.NotFound);
        }

        var dto = new TenantSubscriptionDto(
            subscription.TenantId,
            subscription.PlanId,
            subscription.Plan.Code,
            subscription.Status,
            subscription.PeriodStart,
            subscription.PeriodEnd,
            subscription.TrialEndsAt,
            subscription.TrialEndAction,
            subscription.CreditBalance,
            subscription.AddOns.Select(a => a.AddOn!.Code).ToList());

        return Result.Success(dto);
    }

    /// <inheritdoc />
    public async Task<Result<TenantEntitlementsDto>> Handle(GetTenantEntitlementsQuery request, CancellationToken cancellationToken)
    {
        var modules = await _entitlementReader.GetEnabledModulesAsync(request.TenantId, cancellationToken);
        return Result.Success(new TenantEntitlementsDto(request.TenantId, modules.OrderBy(m => m).ToList()));
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<BillingInvoiceDto>>> Handle(
        ListTenantBillingInvoicesQuery request,
        CancellationToken cancellationToken)
    {
        if (request.TenantId == Guid.Empty)
        {
            return Result.Failure<IReadOnlyList<BillingInvoiceDto>>(PlatformErrorCodes.Billing.InvalidTenant);
        }

        var invoices = await _billingInvoiceRepository.ListByTenantIdAsync(request.TenantId, cancellationToken);
        var dtos = invoices.Select(i => new BillingInvoiceDto(
            i.Id,
            i.PeriodStart,
            i.PeriodEnd,
            i.Amount,
            i.Status,
            i.PaidAt,
            (i.Charges ?? Array.Empty<Platform.Domain.Entities.BillingCharge>())
                .Select(c => new BillingChargeDto(
                    c.GatewayPaymentId,
                    c.PixCopyPaste,
                    c.BoletoIdentificationField)).ToList())).ToList();

        return Result.Success<IReadOnlyList<BillingInvoiceDto>>(dtos);
    }
}
