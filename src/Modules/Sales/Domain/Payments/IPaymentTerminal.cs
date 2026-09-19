using Core.Domain;

namespace Sales.Domain.Payments;

/// <summary>
/// Abstraction for card/Pix terminals (TEF or acquirer APIs). Implementations live in Domain (simulator) or Infrastructure (real adapters).
/// </summary>
public interface IPaymentTerminal
{
    /// <summary>
    /// Authorizes a debit, credit, or Pix charge and returns acquirer references (NSU).
    /// </summary>
    Task<Result<PaymentTerminalAuthorization>> AuthorizeAsync(
        PaymentTerminalAuthorizeRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Voids or refunds an amount against a prior authorization identified by NSU.
    /// </summary>
    Task<Result<PaymentTerminalRefund>> RefundAsync(
        PaymentTerminalRefundRequest request,
        CancellationToken cancellationToken = default);
}
