using Core.Application.Messaging;
using Core.Domain;
using MediatR;
using Microsoft.Extensions.Options;
using Platform.Application.Abstractions;
using Platform.Application.Billing;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;
using Platform.Domain.Services;
using Platform.Application.Configuration;

namespace Platform.Application.Dunning;

/// <summary>Handles SaaS dunning sweep (9.5).</summary>
public sealed class DunningCommandHandlers : IRequestHandler<ProcessDunningCommand, Result<int>>
{
    private readonly ITenantSubscriptionRepository _subscriptionRepository;
    private readonly IBillingInvoiceRepository _invoiceRepository;
    private readonly IBillingCustomerRepository _customerRepository;
    private readonly IBillingPaymentMethodRepository _paymentMethodRepository;
    private readonly IDunningNoticeRepository _noticeRepository;
    private readonly IBillingGateway _billingGateway;
    private readonly IDunningNotifier _notifier;
    private readonly IPlatformUnitOfWork _unitOfWork;
    private readonly DunningOptions _options;

    /// <summary>Creates handlers.</summary>
    public DunningCommandHandlers(
        ITenantSubscriptionRepository subscriptionRepository,
        IBillingInvoiceRepository invoiceRepository,
        IBillingCustomerRepository customerRepository,
        IBillingPaymentMethodRepository paymentMethodRepository,
        IDunningNoticeRepository noticeRepository,
        IBillingGateway billingGateway,
        IDunningNotifier notifier,
        IPlatformUnitOfWork unitOfWork,
        IOptions<DunningOptions> options)
    {
        _subscriptionRepository = subscriptionRepository;
        _invoiceRepository = invoiceRepository;
        _customerRepository = customerRepository;
        _paymentMethodRepository = paymentMethodRepository;
        _noticeRepository = noticeRepository;
        _billingGateway = billingGateway;
        _notifier = notifier;
        _unitOfWork = unitOfWork;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<Result<int>> Handle(ProcessDunningCommand request, CancellationToken cancellationToken)
    {
        var processed = 0;
        var subscriptions = await _subscriptionRepository.ListForDunningAsync(cancellationToken);
        var noticeDays = _options.ParseNoticeDays();

        foreach (var subscription in subscriptions)
        {
            subscription.EvaluateOperationalLock(request.AsOfUtc, _options.LockAfterDays);

            if (subscription.PastDueSince is null)
            {
                continue;
            }

            var invoice = await _invoiceRepository.GetOutstandingByTenantIdAsync(subscription.TenantId, cancellationToken);
            if (invoice is null)
            {
                continue;
            }

            var customer = await _customerRepository.GetByTenantIdAsync(subscription.TenantId, cancellationToken);
            var sentKeys = await _noticeRepository.ListSentKeysAsync(subscription.TenantId, invoice.Id, cancellationToken);
            var dueSteps = DunningScheduleCalculator.GetDueSteps(
                subscription.PastDueSince.Value,
                request.AsOfUtc,
                sentKeys,
                noticeDays);

            foreach (var (stepDay, channel) in dueSteps)
            {
                var noticeResult = DunningNotice.Schedule(
                    subscription.TenantId,
                    invoice.Id,
                    stepDay,
                    channel,
                    request.AsOfUtc);
                if (noticeResult.IsFailure)
                {
                    continue;
                }

                var notice = noticeResult.Value;
                var send = await _notifier.SendAsync(
                    notice,
                    customer?.Email ?? string.Empty,
                    customer?.CpfCnpj,
                    cancellationToken);
                if (send.IsSuccess)
                {
                    await _noticeRepository.AddAsync(notice, cancellationToken);
                }
            }

            var paymentMethod = await _paymentMethodRepository.GetByTenantIdAsync(subscription.TenantId, cancellationToken);
            if (paymentMethod?.Kind == BillingPaymentMethodKind.CreditCard
                && CardRetryPolicy.ShouldRetry(invoice, request.AsOfUtc, _options.CardMaxRetries))
            {
                var charge = await BillingPaymentExecutor.ChargeOutstandingAsync(
                    invoice,
                    subscription,
                    _customerRepository,
                    _paymentMethodRepository,
                    _billingGateway,
                    request.AsOfUtc,
                    cancellationToken);
                if (charge.IsSuccess)
                {
                    CardRetryPolicy.RecordAttemptScheduled(invoice, request.AsOfUtc);
                }
            }

            processed++;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(processed);
    }
}
