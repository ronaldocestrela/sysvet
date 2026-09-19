using Core.Domain;
using Sales.Domain.Enums;
using Sales.Domain.ValueObjects;

namespace Sales.Domain.Entities;

/// <summary>
/// Payment slice recorded when an order is paid (split payments supported).
/// </summary>
public class Payment : Entity
{
    public Guid OrderId { get; private set; }
    public PaymentMethod Method { get; private set; }
    public Money Amount { get; private set; } = Money.Zero;
    public string? Nsu { get; private set; }
    public string? AuthorizationCode { get; private set; }
    public string? Provider { get; private set; }
    public string? TerminalId { get; private set; }
    public string? Brand { get; private set; }
    public int Installments { get; private set; } = 1;

    private readonly List<PaymentRefund> _refunds = new();
    public IReadOnlyCollection<PaymentRefund> Refunds => _refunds.AsReadOnly();

    /// <summary>Amount still eligible for refund on this payment line.</summary>
    public decimal RemainingRefundable => Amount.Amount - _refunds.Sum(r => r.Amount.Amount);

    private Payment() { }

    private Payment(
        Guid orderId,
        PaymentMethod method,
        Money amount,
        string? nsu,
        string? authorizationCode,
        string? provider,
        string? terminalId,
        string? brand,
        int installments)
    {
        OrderId = orderId;
        Method = method;
        Amount = amount;
        Nsu = nsu;
        AuthorizationCode = authorizationCode;
        Provider = provider;
        TerminalId = terminalId;
        Brand = brand;
        Installments = installments;
    }

    /// <summary>
    /// Creates a payment line with a non-negative amount and TEF metadata rules per method.
    /// </summary>
    public static Result<Payment> Create(
        PaymentMethod method,
        decimal amount,
        string? nsu = null,
        string? authorizationCode = null,
        string? provider = null,
        string? terminalId = null,
        string? brand = null,
        int installments = 1)
    {
        var moneyResult = Money.Create(amount);
        if (moneyResult.IsFailure)
        {
            return Result.Failure<Payment>(moneyResult.Error);
        }

        if (moneyResult.Value.Amount == 0)
        {
            return Result.Failure<Payment>(ErrorCodes.Payment.ZeroAmount);
        }

        var tefValidation = ValidateTefMetadata(method, nsu);
        if (tefValidation.IsFailure)
        {
            return Result.Failure<Payment>(tefValidation.Error);
        }

        var normalizedNsu = NormalizeOptional(nsu);
        return Result.Success(new Payment(
            Guid.Empty,
            method,
            moneyResult.Value,
            normalizedNsu,
            NormalizeOptional(authorizationCode),
            NormalizeOptional(provider),
            NormalizeOptional(terminalId),
            NormalizeOptional(brand),
            method == PaymentMethod.CreditCard ? Math.Max(1, installments) : 1));
    }

    internal static Payment Attach(
        Guid orderId,
        PaymentMethod method,
        Money amount,
        string? nsu = null,
        string? authorizationCode = null,
        string? provider = null,
        string? terminalId = null,
        string? brand = null,
        int installments = 1)
    {
        return new Payment(
            orderId,
            method,
            amount,
            NormalizeOptional(nsu),
            NormalizeOptional(authorizationCode),
            NormalizeOptional(provider),
            NormalizeOptional(terminalId),
            NormalizeOptional(brand),
            method == PaymentMethod.CreditCard ? Math.Max(1, installments) : 1);
    }

    /// <summary>Rehydrates a payment from sync pull.</summary>
    public static Payment Restore(
        Guid id,
        Guid orderId,
        PaymentMethod method,
        decimal amount,
        string? nsu,
        string? authorizationCode,
        string? provider,
        string? terminalId,
        string? brand,
        int installments,
        IEnumerable<(Guid RefundId, decimal RefundAmount, string? RefundNsu, DateTimeOffset CreatedAt)>? refunds = null)
    {
        var payment = new Payment(
            id,
            orderId,
            method,
            Money.CreateUnsafe(amount),
            NormalizeOptional(nsu),
            NormalizeOptional(authorizationCode),
            NormalizeOptional(provider),
            NormalizeOptional(terminalId),
            NormalizeOptional(brand),
            installments);

        if (refunds != null)
        {
            foreach (var r in refunds)
            {
                payment._refunds.Add(PaymentRefund.Restore(r.RefundId, id, r.RefundAmount, r.RefundNsu, r.CreatedAt));
            }
        }

        return payment;
    }

    internal Result<PaymentRefund> ApplyRefund(decimal amount, string? refundNsu)
    {
        if (amount <= 0)
        {
            return Result.Failure<PaymentRefund>(ErrorCodes.Payment.ZeroAmount);
        }

        if (amount > RemainingRefundable)
        {
            return Result.Failure<PaymentRefund>(ErrorCodes.Payment.RefundExceedsRemaining);
        }

        if (RequiresTefNsu(Method) && string.IsNullOrWhiteSpace(refundNsu))
        {
            return Result.Failure<PaymentRefund>(ErrorCodes.Payment.NsuRequired);
        }

        var refund = PaymentRefund.Create(Id, amount, refundNsu);
        _refunds.Add(refund);
        return Result.Success(refund);
    }

    /// <summary>Whether the method requires an acquirer NSU on the payment line.</summary>
    public static bool RequiresTefNsu(PaymentMethod method) =>
        method is PaymentMethod.DebitCard or PaymentMethod.CreditCard or PaymentMethod.Pix;

    private static Result<bool> ValidateTefMetadata(PaymentMethod method, string? nsu)
    {
        var hasNsu = !string.IsNullOrWhiteSpace(nsu);
        if (RequiresTefNsu(method) && !hasNsu)
        {
            return Result.Failure<bool>(ErrorCodes.Payment.NsuRequired);
        }

        if (method == PaymentMethod.Cash && hasNsu)
        {
            return Result.Failure<bool>(ErrorCodes.Payment.NsuNotAllowed);
        }

        return Result.Success(true);
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private Payment(
        Guid id,
        Guid orderId,
        PaymentMethod method,
        Money amount,
        string? nsu,
        string? authorizationCode,
        string? provider,
        string? terminalId,
        string? brand,
        int installments)
        : base(id)
    {
        OrderId = orderId;
        Method = method;
        Amount = amount;
        Nsu = nsu;
        AuthorizationCode = authorizationCode;
        Provider = provider;
        TerminalId = terminalId;
        Brand = brand;
        Installments = installments;
    }
}
