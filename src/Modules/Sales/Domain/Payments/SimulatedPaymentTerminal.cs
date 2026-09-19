using Core.Domain;
using Sales.Domain.Enums;

namespace Sales.Domain.Payments;

/// <summary>
/// Offline-capable PoC terminal that generates synthetic NSU values without external I/O.
/// </summary>
public sealed class SimulatedPaymentTerminal : IPaymentTerminal
{
    /// <inheritdoc />
    public Task<Result<PaymentTerminalAuthorization>> AuthorizeAsync(
        PaymentTerminalAuthorizeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Method is PaymentMethod.Cash)
        {
            return Task.FromResult(Result.Failure<PaymentTerminalAuthorization>(
                ErrorCodes.Payment.TerminalFailed));
        }

        if (request.Amount <= 0)
        {
            return Task.FromResult(Result.Failure<PaymentTerminalAuthorization>(
                ErrorCodes.Payment.ZeroAmount));
        }

        var nsu = GenerateNsu();
        var auth = new PaymentTerminalAuthorization(
            Nsu: nsu,
            AuthorizationCode: nsu[^6..],
            Provider: "Simulator",
            TerminalId: "SIM-001",
            Brand: request.Method == PaymentMethod.Pix ? "Pix" : "SimCard",
            Installments: request.Method == PaymentMethod.CreditCard
                ? Math.Max(1, request.Installments)
                : 1);

        return Task.FromResult(Result.Success(auth));
    }

    /// <inheritdoc />
    public Task<Result<PaymentTerminalRefund>> RefundAsync(
        PaymentTerminalRefundRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Method is PaymentMethod.Cash)
        {
            return Task.FromResult(Result.Success(new PaymentTerminalRefund(
                RefundNsu: string.Empty,
                Provider: "Simulator")));
        }

        if (string.IsNullOrWhiteSpace(request.OriginalNsu))
        {
            return Task.FromResult(Result.Failure<PaymentTerminalRefund>(
                ErrorCodes.Payment.NsuRequired));
        }

        if (request.Amount <= 0)
        {
            return Task.FromResult(Result.Failure<PaymentTerminalRefund>(
                ErrorCodes.Payment.ZeroAmount));
        }

        return Task.FromResult(Result.Success(new PaymentTerminalRefund(
            RefundNsu: GenerateNsu(),
            Provider: "Simulator")));
    }

    private static string GenerateNsu()
    {
        var ticks = DateTimeOffset.UtcNow.Ticks % 1_000_000_000_000L;
        return ticks.ToString("D12");
    }
}
