using FluentAssertions;
using Sales.Domain.Entities;
using Sales.Domain.Enums;
using Sales.Domain.Payments;

namespace Sales.Tests.Domain;

public class PaymentTests
{
    [Fact]
    public void Create_Cash_WithNsu_ReturnsFailure()
    {
        var result = Payment.Create(PaymentMethod.Cash, 10m, nsu: "123");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Payment.NsuNotAllowed");
    }

    [Fact]
    public void Create_Pix_WithoutNsu_ReturnsFailure()
    {
        var result = Payment.Create(PaymentMethod.Pix, 10m);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Payment.NsuRequired");
    }

    [Fact]
    public void Create_Pix_WithNsu_ReturnsSuccess()
    {
        var result = Payment.Create(PaymentMethod.Pix, 10m, nsu: "000000000001", provider: "Simulator");

        result.IsSuccess.Should().BeTrue();
        result.Value.Nsu.Should().Be("000000000001");
        result.Value.Provider.Should().Be("Simulator");
    }

}

public class SimulatedPaymentTerminalTests
{
    [Fact]
    public async Task AuthorizeAsync_Pix_ReturnsNsu()
    {
        var terminal = new SimulatedPaymentTerminal();

        var result = await terminal.AuthorizeAsync(new PaymentTerminalAuthorizeRequest(PaymentMethod.Pix, 25m));

        result.IsSuccess.Should().BeTrue();
        result.Value.Nsu.Should().HaveLength(12);
        result.Value.Provider.Should().Be("Simulator");
    }

    [Fact]
    public async Task AuthorizeAsync_Cash_ReturnsFailure()
    {
        var terminal = new SimulatedPaymentTerminal();

        var result = await terminal.AuthorizeAsync(new PaymentTerminalAuthorizeRequest(PaymentMethod.Cash, 10m));

        result.IsFailure.Should().BeTrue();
    }
}
