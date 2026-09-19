using Sales.Domain.Enums;

namespace Sales.Domain.Payments;

/// <summary>
/// Input for authorizing an electronic payment at the terminal (TEF/API).
/// </summary>
public sealed record PaymentTerminalAuthorizeRequest(
    PaymentMethod Method,
    decimal Amount,
    int Installments = 1);

/// <summary>
/// Successful authorization result from the payment terminal.
/// </summary>
public sealed record PaymentTerminalAuthorization(
    string Nsu,
    string AuthorizationCode,
    string Provider,
    string? TerminalId = null,
    string? Brand = null,
    int Installments = 1);

/// <summary>
/// Input for refunding a previously authorized payment at the terminal.
/// </summary>
public sealed record PaymentTerminalRefundRequest(
    PaymentMethod Method,
    decimal Amount,
    string OriginalNsu);

/// <summary>
/// Successful refund result from the payment terminal.
/// </summary>
public sealed record PaymentTerminalRefund(
    string RefundNsu,
    string Provider);
