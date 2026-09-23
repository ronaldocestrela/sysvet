using Core.Application.Messaging;
using Core.Domain;
using MediatR;
using Platform.Application.Abstractions;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;
using Platform.Domain.Services;
using PlatformErrorCodes = Platform.Domain.ErrorCodes;

namespace Platform.Application.Billing;

/// <summary>Billing command handlers (9.4).</summary>
public sealed class BillingCommandHandlers :
    IRequestHandler<ChargeTenantBillingCommand, Result<ChargeTenantBillingResultDto>>,
    IRequestHandler<ChargeDueSubscriptionsCommand, Result<int>>,
    IRequestHandler<UpsertBillingCustomerCommand, Result<BillingCustomerDto>>,
    IRequestHandler<UpsertBillingPaymentMethodCommand, Result<BillingPaymentMethodDto>>,
    IRequestHandler<ProcessAsaasWebhookCommand, Result>
{
    private readonly ITenantSubscriptionRepository _subscriptionRepository;
    private readonly ISubscriptionAdjustmentRepository _adjustmentRepository;
    private readonly IBillingCustomerRepository _customerRepository;
    private readonly IBillingPaymentMethodRepository _paymentMethodRepository;
    private readonly IBillingInvoiceRepository _invoiceRepository;
    private readonly IBillingWebhookReceiptRepository _webhookReceiptRepository;
    private readonly IBillingGateway _billingGateway;
    private readonly IPlatformUnitOfWork _unitOfWork;
    private readonly IBillingWebhookAuthenticator _webhookAuthenticator;
    private readonly IMediator _mediator;

    /// <summary>Creates handlers.</summary>
    public BillingCommandHandlers(
        ITenantSubscriptionRepository subscriptionRepository,
        ISubscriptionAdjustmentRepository adjustmentRepository,
        IBillingCustomerRepository customerRepository,
        IBillingPaymentMethodRepository paymentMethodRepository,
        IBillingInvoiceRepository invoiceRepository,
        IBillingWebhookReceiptRepository webhookReceiptRepository,
        IBillingGateway billingGateway,
        IPlatformUnitOfWork unitOfWork,
        IBillingWebhookAuthenticator webhookAuthenticator,
        IMediator mediator)
    {
        _subscriptionRepository = subscriptionRepository;
        _adjustmentRepository = adjustmentRepository;
        _customerRepository = customerRepository;
        _paymentMethodRepository = paymentMethodRepository;
        _invoiceRepository = invoiceRepository;
        _webhookReceiptRepository = webhookReceiptRepository;
        _billingGateway = billingGateway;
        _unitOfWork = unitOfWork;
        _webhookAuthenticator = webhookAuthenticator;
        _mediator = mediator;
    }

    /// <inheritdoc />
    public async Task<Result<ChargeTenantBillingResultDto>> Handle(
        ChargeTenantBillingCommand request,
        CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetByTenantIdAsync(request.TenantId, cancellationToken);
        if (subscription?.Plan is null)
        {
            return Result.Failure<ChargeTenantBillingResultDto>(PlatformErrorCodes.Subscription.NotFound);
        }

        if (!subscription.IsDueForBilling(request.AsOfUtc))
        {
            return Result.Failure<ChargeTenantBillingResultDto>(PlatformErrorCodes.Billing.NotDue);
        }

        var openInvoice = await _invoiceRepository.GetOpenByTenantIdAsync(request.TenantId, cancellationToken);
        if (openInvoice is not null)
        {
            return Result.Failure<ChargeTenantBillingResultDto>(PlatformErrorCodes.Billing.OpenInvoiceExists);
        }

        var pendingAdjustments = await _adjustmentRepository.ListPendingByTenantIdAsync(request.TenantId, cancellationToken);
        var addOnPrices = subscription.AddOns
            .Where(a => a.AddOn is not null)
            .Select(a => a.AddOn!.MonthlyPrice);
        var amount = BillingInvoiceComposer.ComposeAmount(
            subscription.Plan.MonthlyPrice,
            addOnPrices,
            pendingAdjustments);

        var invoiceResult = BillingInvoice.Open(
            request.TenantId,
            subscription.PeriodStart,
            subscription.PeriodEnd,
            amount);
        if (invoiceResult.IsFailure)
        {
            return Result.Failure<ChargeTenantBillingResultDto>(invoiceResult.Error);
        }

        var invoice = invoiceResult.Value;
        foreach (var adjustment in pendingAdjustments.Where(a => a.Amount > 0))
        {
            var invoiced = adjustment.MarkInvoiced(invoice.Id);
            if (invoiced.IsFailure)
            {
                return Result.Failure<ChargeTenantBillingResultDto>(invoiced.Error);
            }
        }

        subscription.MarkInvoiced();
        await _invoiceRepository.AddAsync(invoice, cancellationToken);

        if (amount == 0)
        {
            var settled = await BillingInvoiceSettlement.ApplyPaidAsync(
                invoice,
                subscription,
                _adjustmentRepository,
                request.AsOfUtc,
                cancellationToken);
            if (settled.IsFailure)
            {
                return Result.Failure<ChargeTenantBillingResultDto>(settled.Error);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(new ChargeTenantBillingResultDto(invoice.Id, 0m, invoice.Status, null));
        }

        var customer = await _customerRepository.GetByTenantIdAsync(request.TenantId, cancellationToken);
        if (customer is null || string.IsNullOrWhiteSpace(customer.GatewayCustomerId))
        {
            return Result.Failure<ChargeTenantBillingResultDto>(PlatformErrorCodes.Billing.CustomerNotFound);
        }

        var paymentMethod = await _paymentMethodRepository.GetByTenantIdAsync(request.TenantId, cancellationToken);
        if (paymentMethod is null)
        {
            return Result.Failure<ChargeTenantBillingResultDto>(PlatformErrorCodes.Billing.PaymentMethodNotFound);
        }

        var dueDate = DateOnly.FromDateTime(request.AsOfUtc.UtcDateTime.AddDays(3));
        var paymentRequest = new BillingGatewayPaymentRequest(
            invoice.Id,
            customer.GatewayCustomerId,
            amount,
            dueDate,
            paymentMethod.Kind,
            paymentMethod.CreditCardToken);

        var gatewayPayment = await _billingGateway.CreatePaymentAsync(paymentRequest, cancellationToken);
        if (gatewayPayment.IsFailure)
        {
            return Result.Failure<ChargeTenantBillingResultDto>(gatewayPayment.Error);
        }

        invoice.AddCharge(
            gatewayPayment.Value.GatewayPaymentId,
            gatewayPayment.Value.PixCopyPaste,
            gatewayPayment.Value.BoletoIdentificationField);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(new ChargeTenantBillingResultDto(
            invoice.Id,
            amount,
            invoice.Status,
            gatewayPayment.Value.GatewayPaymentId));
    }

    /// <inheritdoc />
    public async Task<Result<int>> Handle(ChargeDueSubscriptionsCommand request, CancellationToken cancellationToken)
    {
        var due = await _subscriptionRepository.ListDueForBillingAsync(request.AsOfUtc, cancellationToken);
        var charged = 0;
        foreach (var subscription in due)
        {
            var result = await _mediator.Send(
                new ChargeTenantBillingCommand(subscription.TenantId, request.AsOfUtc),
                cancellationToken);
            if (result.IsSuccess)
            {
                charged++;
            }
        }

        return Result.Success(charged);
    }

    /// <inheritdoc />
    public async Task<Result<BillingCustomerDto>> Handle(
        UpsertBillingCustomerCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await _customerRepository.GetByTenantIdAsync(request.TenantId, cancellationToken);
        BillingCustomer customer;
        if (existing is null)
        {
            var created = BillingCustomer.Create(request.TenantId, request.Name, request.Email, request.CpfCnpj);
            if (created.IsFailure)
            {
                return Result.Failure<BillingCustomerDto>(created.Error);
            }

            customer = created.Value;
            await _customerRepository.AddAsync(customer, cancellationToken);
        }
        else
        {
            var updated = existing.Update(request.Name, request.Email, request.CpfCnpj);
            if (updated.IsFailure)
            {
                return Result.Failure<BillingCustomerDto>(updated.Error);
            }

            customer = existing;
        }

        var gatewayResult = await _billingGateway.EnsureCustomerAsync(
            new BillingGatewayCustomerRequest(
                customer.TenantId,
                customer.Name,
                customer.Email,
                customer.CpfCnpj,
                customer.GatewayCustomerId),
            cancellationToken);
        if (gatewayResult.IsFailure)
        {
            return Result.Failure<BillingCustomerDto>(gatewayResult.Error);
        }

        customer.SetGatewayCustomerId(gatewayResult.Value);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(new BillingCustomerDto(
            customer.TenantId,
            customer.Name,
            customer.Email,
            customer.CpfCnpj,
            customer.GatewayCustomerId));
    }

    /// <inheritdoc />
    public async Task<Result<BillingPaymentMethodDto>> Handle(
        UpsertBillingPaymentMethodCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await _paymentMethodRepository.GetByTenantIdAsync(request.TenantId, cancellationToken);
        if (existing is null)
        {
            var created = BillingPaymentMethod.Create(request.TenantId, request.Kind, request.CreditCardToken);
            if (created.IsFailure)
            {
                return Result.Failure<BillingPaymentMethodDto>(created.Error);
            }

            await _paymentMethodRepository.AddAsync(created.Value, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(new BillingPaymentMethodDto(
                request.TenantId,
                created.Value.Kind,
                !string.IsNullOrWhiteSpace(created.Value.CreditCardToken)));
        }

        var updated = existing.Update(request.Kind, request.CreditCardToken);
        if (updated.IsFailure)
        {
            return Result.Failure<BillingPaymentMethodDto>(updated.Error);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(new BillingPaymentMethodDto(
            existing.TenantId,
            existing.Kind,
            !string.IsNullOrWhiteSpace(existing.CreditCardToken)));
    }

    /// <inheritdoc />
    public async Task<Result> Handle(ProcessAsaasWebhookCommand request, CancellationToken cancellationToken)
    {
        if (!_webhookAuthenticator.IsAuthorized(request.AccessToken))
        {
            return Result.Failure(PlatformErrorCodes.Billing.WebhookUnauthorized);
        }

        var payload = request.Payload;
        if (string.IsNullOrWhiteSpace(payload.Event) || payload.Payment?.Id is null)
        {
            return Result.Failure(PlatformErrorCodes.Billing.WebhookInvalidPayload);
        }

        var idempotencyKey = $"{payload.Event}:{payload.Payment.Id}";
        if (await _webhookReceiptRepository.ExistsAsync(idempotencyKey, cancellationToken))
        {
            return Result.Success();
        }

        if (!Guid.TryParse(payload.Payment.ExternalReference, out var invoiceId))
        {
            return Result.Failure(PlatformErrorCodes.Billing.InvoiceNotFound);
        }

        var invoice = await _invoiceRepository.GetByIdAsync(invoiceId, cancellationToken);
        if (invoice is null)
        {
            return Result.Failure(PlatformErrorCodes.Billing.InvoiceNotFound);
        }

        var subscription = await _subscriptionRepository.GetByTenantIdAsync(invoice.TenantId, cancellationToken);
        if (subscription is null)
        {
            return Result.Failure(PlatformErrorCodes.Subscription.NotFound);
        }

        var asOf = DateTimeOffset.UtcNow;
        var eventName = payload.Event.ToUpperInvariant();
        Result applyResult = eventName switch
        {
            "PAYMENT_RECEIVED" or "PAYMENT_CONFIRMED" => await BillingInvoiceSettlement.ApplyPaidAsync(
                invoice,
                subscription,
                _adjustmentRepository,
                asOf,
                cancellationToken),
            "PAYMENT_OVERDUE" => ApplyOverdue(invoice, subscription, asOf),
            "PAYMENT_DELETED" => invoice.Cancel(),
            "PAYMENT_REFUNDED" => invoice.MarkRefunded(),
            _ => Result.Success()
        };

        if (applyResult.IsFailure)
        {
            return applyResult;
        }

        await _webhookReceiptRepository.AddAsync(
            BillingWebhookReceipt.Create(idempotencyKey, asOf),
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private static Result ApplyOverdue(BillingInvoice invoice, TenantSubscription subscription, DateTimeOffset asOfUtc)
    {
        var failed = invoice.MarkFailed();
        if (failed.IsFailure)
        {
            return failed;
        }

        return subscription.RecordPaymentOverdue(asOfUtc);
    }

}
