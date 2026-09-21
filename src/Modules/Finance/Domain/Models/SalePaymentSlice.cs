namespace Finance.Domain.Models;

/// <summary>
/// Payment slice from a paid order used to settle receivable titles.
/// </summary>
public sealed record SalePaymentSlice(string Method, decimal Amount, string? Nsu, Guid CorrelationId);
