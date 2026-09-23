using Core.Application.Messaging;
using Core.Domain;
using MediatR;
using Platform.Application.Abstractions;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;
using PlatformErrorCodes = Platform.Domain.ErrorCodes;

namespace Platform.Application.Billing;

/// <summary>Clinic-facing SaaS billing handlers (9.5).</summary>
public sealed class ClinicBillingCommandHandlers :
    IRequestHandler<GetClinicBillingStandingQuery, Result<ClinicBillingStandingDto>>,
    IRequestHandler<PayClinicBillingCommand, Result<PayClinicBillingResultDto>>
{
    private readonly ITenantSubscriptionRepository _subscriptionRepository;
    private readonly IBillingInvoiceRepository _invoiceRepository;
    private readonly IBillingCustomerRepository _customerRepository;
    private readonly IBillingPaymentMethodRepository _paymentMethodRepository;
    private readonly IBillingGateway _billingGateway;
    private readonly IPlatformUnitOfWork _unitOfWork;

    /// <summary>Creates handlers.</summary>
    public ClinicBillingCommandHandlers(
        ITenantSubscriptionRepository subscriptionRepository,
        IBillingInvoiceRepository invoiceRepository,
        IBillingCustomerRepository customerRepository,
        IBillingPaymentMethodRepository paymentMethodRepository,
        IBillingGateway billingGateway,
        IPlatformUnitOfWork unitOfWork)
    {
        _subscriptionRepository = subscriptionRepository;
        _invoiceRepository = invoiceRepository;
        _customerRepository = customerRepository;
        _paymentMethodRepository = paymentMethodRepository;
        _billingGateway = billingGateway;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<Result<ClinicBillingStandingDto>> Handle(
        GetClinicBillingStandingQuery request,
        CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetByTenantIdAsync(request.TenantId, cancellationToken);
        if (subscription is null)
        {
            return Result.Failure<ClinicBillingStandingDto>(PlatformErrorCodes.Subscription.NotFound);
        }

        var invoice = await _invoiceRepository.GetOutstandingByTenantIdAsync(request.TenantId, cancellationToken);
        var latestCharge = invoice?.Charges.OrderByDescending(c => c.UpdatedAt).FirstOrDefault();

        return Result.Success(new ClinicBillingStandingDto(
            subscription.BillingStanding,
            subscription.IsOperationallyLocked,
            invoice?.Id,
            invoice?.Amount,
            invoice?.Status,
            latestCharge?.PixCopyPaste,
            latestCharge?.BoletoIdentificationField,
            latestCharge?.GatewayPaymentId));
    }

    /// <inheritdoc />
    public async Task<Result<PayClinicBillingResultDto>> Handle(
        PayClinicBillingCommand request,
        CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetByTenantIdAsync(request.TenantId, cancellationToken);
        if (subscription is null)
        {
            return Result.Failure<PayClinicBillingResultDto>(PlatformErrorCodes.Subscription.NotFound);
        }

        var invoice = await _invoiceRepository.GetOutstandingByTenantIdAsync(request.TenantId, cancellationToken);
        if (invoice is null)
        {
            return Result.Failure<PayClinicBillingResultDto>(PlatformErrorCodes.Billing.NoOutstandingInvoice);
        }

        var charge = await BillingPaymentExecutor.ChargeOutstandingAsync(
            invoice,
            subscription,
            _customerRepository,
            _paymentMethodRepository,
            _billingGateway,
            request.AsOfUtc,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        if (charge.IsFailure)
        {
            return Result.Failure<PayClinicBillingResultDto>(charge.Error);
        }

        return Result.Success(new PayClinicBillingResultDto(invoice.Id, invoice.Amount, charge.Value));
    }
}
