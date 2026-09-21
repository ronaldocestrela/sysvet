using Core.Domain;
using Finance.Domain.Enums;
using Finance.Domain.ValueObjects;

namespace Finance.Domain.Entities;

/// <summary>
/// Payment or reversal applied against a financial title.
/// </summary>
public sealed class TitleAllocation : Entity
{
    public Guid FinancialTitleId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTimeOffset PaidAt { get; private set; }
    public string Method { get; private set; } = string.Empty;
    public Guid CorrelationId { get; private set; }
    public AllocationKind Kind { get; private set; }

    private TitleAllocation() { }

    internal static TitleAllocation CreateSettlement(
        Guid titleId,
        decimal amount,
        DateTimeOffset paidAt,
        string method,
        Guid correlationId,
        Guid? id = null)
    {
        return new TitleAllocation
        {
            Id = id ?? Guid.NewGuid(),
            FinancialTitleId = titleId,
            Amount = amount,
            PaidAt = paidAt,
            Method = method.Trim(),
            CorrelationId = correlationId,
            Kind = AllocationKind.Settlement
        };
    }

    internal static TitleAllocation CreateReversal(
        Guid titleId,
        decimal amount,
        DateTimeOffset paidAt,
        string method,
        Guid correlationId,
        Guid? id = null)
    {
        return new TitleAllocation
        {
            Id = id ?? Guid.NewGuid(),
            FinancialTitleId = titleId,
            Amount = amount,
            PaidAt = paidAt,
            Method = method.Trim(),
            CorrelationId = correlationId,
            Kind = AllocationKind.Reversal
        };
    }

    /// <summary>Rehydrates from persistence or sync.</summary>
    public static TitleAllocation Restore(
        Guid id,
        Guid titleId,
        decimal amount,
        DateTimeOffset paidAt,
        string method,
        Guid correlationId,
        AllocationKind kind,
        DateTimeOffset updatedAt)
    {
        return new TitleAllocation
        {
            Id = id,
            FinancialTitleId = titleId,
            Amount = amount,
            PaidAt = paidAt,
            Method = method,
            CorrelationId = correlationId,
            Kind = kind,
            UpdatedAt = updatedAt
        };
    }
}
