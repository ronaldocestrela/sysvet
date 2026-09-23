using Core.Domain;
using FluentAssertions;
using NSubstitute;
using Platform.Application.Abstractions;
using Platform.Application.Billing;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;
using Platform.Domain.Services;
using PlatformErrorCodes = Platform.Domain.ErrorCodes;
using Xunit;

namespace Platform.Tests.Application.Billing;

public class BillingPaymentExecutorTests
{
    [Fact]
    public async Task ChargeOutstandingAsync_WhenGatewayFails_NotifiesObserver()
    {
        var tenantId = Guid.NewGuid();
        var invoice = BillingInvoice.Open(
            tenantId,
            DateTimeOffset.UtcNow.AddDays(-30),
            DateTimeOffset.UtcNow,
            100m).Value;
        var subscription = TenantSubscription.CreateActive(tenantId, Guid.NewGuid(), DateTimeOffset.UtcNow).Value;

        var customerRepository = Substitute.For<IBillingCustomerRepository>();
        var customer = BillingCustomer.Create(tenantId, "Acme Clinic", "a@acme.com", "12345678901").Value;
        customer.SetGatewayCustomerId("gw-customer");
        customerRepository.GetByTenantIdAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns(customer);

        var paymentMethodRepository = Substitute.For<IBillingPaymentMethodRepository>();
        paymentMethodRepository.GetByTenantIdAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns(BillingPaymentMethod.Create(tenantId, BillingPaymentMethodKind.CreditCard, "token").Value);

        var gateway = Substitute.For<IBillingGateway>();
        gateway.CreatePaymentAsync(Arg.Any<BillingGatewayPaymentRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<BillingGatewayPaymentResult>(PlatformErrorCodes.Billing.GatewayFailed));

        var observer = Substitute.For<IBillingChargeObserver>();

        var result = await BillingPaymentExecutor.ChargeOutstandingAsync(
            invoice,
            subscription,
            customerRepository,
            paymentMethodRepository,
            gateway,
            DateTimeOffset.UtcNow,
            CancellationToken.None,
            observer);

        result.IsFailure.Should().BeTrue();
        invoice.Status.Should().Be(BillingInvoiceStatus.Failed);
        observer.Received(1).OnChargeFailure(tenantId, invoice.Id);
    }

    [Fact]
    public async Task ChargeOutstandingAsync_WhenGatewaySucceeds_DoesNotNotifyObserver()
    {
        var tenantId = Guid.NewGuid();
        var invoice = BillingInvoice.Open(
            tenantId,
            DateTimeOffset.UtcNow.AddDays(-30),
            DateTimeOffset.UtcNow,
            100m).Value;
        var subscription = TenantSubscription.CreateActive(tenantId, Guid.NewGuid(), DateTimeOffset.UtcNow).Value;

        var customerRepository = Substitute.For<IBillingCustomerRepository>();
        var customer = BillingCustomer.Create(tenantId, "Acme Clinic", "a@acme.com", "12345678901").Value;
        customer.SetGatewayCustomerId("gw-customer");
        customerRepository.GetByTenantIdAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns(customer);

        var paymentMethodRepository = Substitute.For<IBillingPaymentMethodRepository>();
        paymentMethodRepository.GetByTenantIdAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns(BillingPaymentMethod.Create(tenantId, BillingPaymentMethodKind.CreditCard, "token").Value);

        var gateway = Substitute.For<IBillingGateway>();
        gateway.CreatePaymentAsync(Arg.Any<BillingGatewayPaymentRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new BillingGatewayPaymentResult("pay-1", null, null)));

        var observer = Substitute.For<IBillingChargeObserver>();

        var result = await BillingPaymentExecutor.ChargeOutstandingAsync(
            invoice,
            subscription,
            customerRepository,
            paymentMethodRepository,
            gateway,
            DateTimeOffset.UtcNow,
            CancellationToken.None,
            observer);

        result.IsSuccess.Should().BeTrue();
        observer.DidNotReceive().OnChargeFailure(Arg.Any<Guid>(), Arg.Any<Guid>());
    }
}
