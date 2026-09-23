using Core.Domain;
using Platform.Application.Abstractions;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;
using Platform.Domain.Services;
using PlatformErrorCodes = Platform.Domain.ErrorCodes;

namespace Platform.Application.Billing;

/// <summary>Shared gateway charge for open or failed invoices (9.4/9.5).</summary>
public static class BillingPaymentExecutor
{
    /// <summary>Creates a gateway payment for an outstanding invoice.</summary>
    public static async Task<Result<string>> ChargeOutstandingAsync(
        BillingInvoice invoice,
        TenantSubscription subscription,
        IBillingCustomerRepository customerRepository,
        IBillingPaymentMethodRepository paymentMethodRepository,
        IBillingGateway billingGateway,
        DateTimeOffset asOfUtc,
        CancellationToken cancellationToken)
    {
        if (!invoice.IsOutstanding)
        {
            return Result.Failure<string>(PlatformErrorCodes.Billing.NoOutstandingInvoice);
        }

        if (invoice.Amount == 0)
        {
            return Result.Failure<string>(PlatformErrorCodes.Billing.InvalidAmount);
        }

        var customer = await customerRepository.GetByTenantIdAsync(subscription.TenantId, cancellationToken);
        if (customer is null || string.IsNullOrWhiteSpace(customer.GatewayCustomerId))
        {
            return Result.Failure<string>(PlatformErrorCodes.Billing.CustomerNotFound);
        }

        var paymentMethod = await paymentMethodRepository.GetByTenantIdAsync(subscription.TenantId, cancellationToken);
        if (paymentMethod is null)
        {
            return Result.Failure<string>(PlatformErrorCodes.Billing.PaymentMethodNotFound);
        }

        var dueDate = DateOnly.FromDateTime(asOfUtc.UtcDateTime.AddDays(3));
        var paymentRequest = new BillingGatewayPaymentRequest(
            invoice.Id,
            customer.GatewayCustomerId,
            invoice.Amount,
            dueDate,
            paymentMethod.Kind,
            paymentMethod.CreditCardToken);

        var gatewayPayment = await billingGateway.CreatePaymentAsync(paymentRequest, cancellationToken);
        if (gatewayPayment.IsFailure)
        {
            invoice.MarkFailed();
            subscription.RecordPaymentOverdue(asOfUtc);
            return Result.Failure<string>(gatewayPayment.Error);
        }

        invoice.AddCharge(
            gatewayPayment.Value.GatewayPaymentId,
            gatewayPayment.Value.PixCopyPaste,
            gatewayPayment.Value.BoletoIdentificationField);

        return Result.Success(gatewayPayment.Value.GatewayPaymentId);
    }
}
