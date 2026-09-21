using Core.Domain;
using Finance.Domain.Enums;
using Finance.Domain.Models;

namespace Finance.Domain.Entities;

/// <summary>
/// Aggregate root for accounts payable or receivable titles.
/// </summary>
public sealed class FinancialTitle : AggregateRoot
{
    public TitleDirection Direction { get; private set; }
    public TitleStatus Status { get; private set; }
    public TitleSourceType SourceType { get; private set; }
    public Guid? SourceId { get; private set; }
    public string SourceInstallmentKey { get; private set; } = "-";
    public PartyKind PartyKind { get; private set; }
    public Guid? PartyId { get; private set; }
    public Guid CategoryId { get; private set; }
    public Guid? CostCenterId { get; private set; }
    public DateOnly IssueDate { get; private set; }
    public DateOnly DueDate { get; private set; }
    public decimal OriginalAmount { get; private set; }
    public string Description { get; private set; } = string.Empty;

    private readonly List<TitleAllocation> _allocations = new();

    /// <summary>Allocations (EF navigation).</summary>
    public IReadOnlyCollection<TitleAllocation> Allocations => _allocations.AsReadOnly();

    private FinancialTitle() { }

    /// <summary>Net settled amount after reversals.</summary>
    public decimal SettledAmount =>
        _allocations.Where(a => a.Kind == AllocationKind.Settlement).Sum(a => a.Amount)
        - _allocations.Where(a => a.Kind == AllocationKind.Reversal).Sum(a => a.Amount);

    /// <summary>Remaining open balance.</summary>
    public decimal OpenAmount => Math.Max(0m, OriginalAmount - SettledAmount);

    /// <summary>Creates a receivable from a paid sale with immediate settlement allocations.</summary>
    public static Result<FinancialTitle> CreateFromSale(
        Guid orderId,
        Guid categoryId,
        decimal totalAmount,
        Guid? tutorId,
        string description,
        IReadOnlyList<SalePaymentSlice> payments,
        DateOnly issueDate,
        Guid? id = null)
    {
        var amountResult = ValidateAmount(totalAmount);
        if (amountResult.IsFailure)
        {
            return Result.Failure<FinancialTitle>(amountResult.Error);
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return Result.Failure<FinancialTitle>(ErrorCodes.Title.InvalidDescription);
        }

        var (partyKind, partyId) = tutorId is { } t && t != Guid.Empty
            ? (PartyKind.Tutor, (Guid?)t)
            : (PartyKind.None, null);

        var titleId = id ?? Guid.NewGuid();
        var title = new FinancialTitle
        {
            Id = titleId,
            Direction = TitleDirection.Receivable,
            Status = TitleStatus.Open,
            SourceType = TitleSourceType.Sale,
            SourceId = orderId,
            SourceInstallmentKey = "-",
            PartyKind = partyKind,
            PartyId = partyId,
            CategoryId = categoryId,
            IssueDate = issueDate,
            DueDate = issueDate,
            OriginalAmount = totalAmount,
            Description = description.Trim()
        };

        var paidAt = DateTimeOffset.UtcNow;
        foreach (var payment in payments)
        {
            if (payment.Amount <= 0)
            {
                continue;
            }

            title._allocations.Add(TitleAllocation.CreateSettlement(
                titleId,
                payment.Amount,
                paidAt,
                payment.Method,
                payment.CorrelationId));
        }

        title.RefreshStatus();
        return Result.Success(title);
    }

    /// <summary>Creates a payable from a purchase NF-e duplicate line.</summary>
    public static Result<FinancialTitle> CreateFromPurchaseDuplicate(
        Guid importId,
        Guid categoryId,
        Guid supplierId,
        string installmentKey,
        decimal amount,
        DateOnly issueDate,
        DateOnly dueDate,
        string description,
        Guid? id = null)
    {
        var amountResult = ValidateAmount(amount);
        if (amountResult.IsFailure)
        {
            return Result.Failure<FinancialTitle>(amountResult.Error);
        }

        if (supplierId == Guid.Empty)
        {
            return Result.Failure<FinancialTitle>(ErrorCodes.Title.InvalidParty);
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return Result.Failure<FinancialTitle>(ErrorCodes.Title.InvalidDescription);
        }

        return Result.Success(new FinancialTitle
        {
            Id = id ?? Guid.NewGuid(),
            Direction = TitleDirection.Payable,
            Status = TitleStatus.Open,
            SourceType = TitleSourceType.Purchase,
            SourceId = importId,
            SourceInstallmentKey = string.IsNullOrWhiteSpace(installmentKey) ? "-" : installmentKey.Trim(),
            PartyKind = PartyKind.Supplier,
            PartyId = supplierId,
            CategoryId = categoryId,
            IssueDate = issueDate,
            DueDate = dueDate,
            OriginalAmount = amount,
            Description = description.Trim()
        });
    }

    /// <summary>Creates a manual payable or receivable.</summary>
    public static Result<FinancialTitle> CreateManual(
        TitleDirection direction,
        Guid categoryId,
        PartyKind partyKind,
        Guid? partyId,
        decimal amount,
        DateOnly issueDate,
        DateOnly dueDate,
        string description,
        Guid? costCenterId = null,
        Guid? id = null)
    {
        var amountResult = ValidateAmount(amount);
        if (amountResult.IsFailure)
        {
            return Result.Failure<FinancialTitle>(amountResult.Error);
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return Result.Failure<FinancialTitle>(ErrorCodes.Title.InvalidDescription);
        }

        if (partyKind is PartyKind.Tutor or PartyKind.Supplier && (partyId is null || partyId == Guid.Empty))
        {
            return Result.Failure<FinancialTitle>(ErrorCodes.Title.InvalidParty);
        }

        var titleId = id ?? Guid.NewGuid();
        return Result.Success(new FinancialTitle
        {
            Id = titleId,
            Direction = direction,
            Status = TitleStatus.Open,
            SourceType = TitleSourceType.Manual,
            SourceId = null,
            SourceInstallmentKey = titleId.ToString("N"),
            PartyKind = partyKind,
            PartyId = partyId,
            CategoryId = categoryId,
            CostCenterId = costCenterId,
            IssueDate = issueDate,
            DueDate = dueDate,
            OriginalAmount = amount,
            Description = description.Trim()
        });
    }

    /// <summary>Records a settlement against an open title.</summary>
    public Result Allocate(decimal amount, DateTimeOffset paidAt, string method, Guid correlationId)
    {
        if (Status is TitleStatus.Settled or TitleStatus.Cancelled)
        {
            return Result.Failure(ErrorCodes.Title.SettleNotAllowed);
        }

        if (amount <= 0)
        {
            return Result.Failure(ErrorCodes.Title.InvalidAmount);
        }

        if (amount > OpenAmount)
        {
            return Result.Failure(ErrorCodes.Title.AllocationExceedsOpen);
        }

        _allocations.Add(TitleAllocation.CreateSettlement(Id, amount, paidAt, method, correlationId));
        RefreshStatus();
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>Reverses settlement for refunds or returns (idempotent by correlation).</summary>
    public Result ReverseAllocation(decimal amount, DateTimeOffset paidAt, string method, Guid correlationId)
    {
        if (_allocations.Any(a => a.CorrelationId == correlationId && a.Kind == AllocationKind.Reversal))
        {
            return Result.Success();
        }

        if (amount <= 0)
        {
            return Result.Failure(ErrorCodes.Title.InvalidAmount);
        }

        if (amount > SettledAmount)
        {
            return Result.Failure(ErrorCodes.Title.AllocationExceedsOpen);
        }

        _allocations.Add(TitleAllocation.CreateReversal(Id, amount, paidAt, method, correlationId));
        RefreshStatus();
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>Cancels manual titles or open integration titles without allocations.</summary>
    public Result Cancel()
    {
        if (Status == TitleStatus.Cancelled)
        {
            return Result.Success();
        }

        if (SourceType != TitleSourceType.Manual && SettledAmount > 0)
        {
            return Result.Failure(ErrorCodes.Title.CancelNotAllowed);
        }

        if (SourceType != TitleSourceType.Manual && _allocations.Count > 0)
        {
            return Result.Failure(ErrorCodes.Title.CancelNotAllowed);
        }

        Status = TitleStatus.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>Rehydrates from sync pull.</summary>
    public static FinancialTitle RestoreFromSync(
        Guid id,
        TitleDirection direction,
        TitleStatus status,
        TitleSourceType sourceType,
        Guid? sourceId,
        string sourceInstallmentKey,
        PartyKind partyKind,
        Guid? partyId,
        Guid categoryId,
        Guid? costCenterId,
        DateOnly issueDate,
        DateOnly dueDate,
        decimal originalAmount,
        string description,
        DateTimeOffset updatedAt,
        IEnumerable<TitleAllocation> allocations)
    {
        var title = new FinancialTitle
        {
            Id = id,
            Direction = direction,
            Status = status,
            SourceType = sourceType,
            SourceId = sourceId,
            SourceInstallmentKey = sourceInstallmentKey,
            PartyKind = partyKind,
            PartyId = partyId,
            CategoryId = categoryId,
            CostCenterId = costCenterId,
            IssueDate = issueDate,
            DueDate = dueDate,
            OriginalAmount = originalAmount,
            Description = description,
            UpdatedAt = updatedAt
        };

        title._allocations.AddRange(allocations);
        return title;
    }

    private void RefreshStatus()
    {
        if (Status == TitleStatus.Cancelled)
        {
            return;
        }

        var settled = SettledAmount;
        if (settled <= 0)
        {
            Status = TitleStatus.Open;
        }
        else if (settled >= OriginalAmount)
        {
            Status = TitleStatus.Settled;
        }
        else
        {
            Status = TitleStatus.PartiallySettled;
        }
    }

    private static Result ValidateAmount(decimal amount) =>
        amount > 0 ? Result.Success() : Result.Failure(ErrorCodes.Title.InvalidAmount);
}
