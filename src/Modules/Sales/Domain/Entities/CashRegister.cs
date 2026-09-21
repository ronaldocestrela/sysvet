using Core.Domain;
using Sales.Domain.Enums;
using Sales.Domain.ValueObjects;

namespace Sales.Domain.Entities;

/// <summary>
/// Cash register session opened by an operator before PDV sales.
/// </summary>
public class CashRegister : AggregateRoot
{
    public Guid OpenedByUserId { get; private set; }
    public DateTimeOffset OpenedAt { get; private set; }
    public DateTimeOffset? ClosedAt { get; private set; }
    public Money OpeningBalance { get; private set; } = Money.Zero;
    public Money ExpectedClosingBalance { get; private set; } = Money.Zero;
    public Money ClosingBalance { get; private set; } = Money.Zero;
    public CashRegisterStatus Status { get; private set; } = CashRegisterStatus.Closed;

    private readonly List<CashMovement> _movements = new();

    /// <summary>Cash drawer movements (sangria/suprimento).</summary>
    public IReadOnlyCollection<CashMovement> Movements => _movements.AsReadOnly();

    private CashRegister() { }

    private CashRegister(Guid id, Guid openedByUserId, Money openingBalance)
        : base(id)
    {
        OpenedByUserId = openedByUserId;
        OpeningBalance = openingBalance;
        OpenedAt = DateTimeOffset.UtcNow;
        Status = CashRegisterStatus.Open;
    }

    /// <summary>
    /// Opens a new register session for the operator (server-generated id).
    /// </summary>
    public static Result<CashRegister> Open(Guid userId, decimal openingBalance)
        => Open(Guid.NewGuid(), userId, openingBalance);

    /// <summary>
    /// Opens a register session with a client-assigned id for offline sync (ADR-026).
    /// </summary>
    public static Result<CashRegister> Open(Guid id, Guid userId, decimal openingBalance)
    {
        if (id == Guid.Empty)
        {
            return Result.Failure<CashRegister>(ErrorCodes.CashRegister.InvalidId);
        }

        var moneyResult = Money.Create(openingBalance);
        if (!moneyResult.IsSuccess)
        {
            return Result.Failure<CashRegister>(moneyResult.Error);
        }

        return Result.Success(new CashRegister(id, userId, moneyResult.Value));
    }

    /// <summary>
    /// Computes expected cash in drawer: opening + cash sales net − drops + supplies.
    /// </summary>
    public decimal ComputeExpectedCash(decimal cashNet)
    {
        var drops = _movements.Where(m => m.Kind == CashMovementKind.Drop).Sum(m => m.Amount.Amount);
        var supplies = _movements.Where(m => m.Kind == CashMovementKind.Supply).Sum(m => m.Amount.Amount);
        return OpeningBalance.Amount + cashNet - drops + supplies;
    }

    /// <summary>Records a sangria (cash removed from drawer).</summary>
    public Result<Guid> RecordDrop(decimal amount, string reason, decimal cashNet, Guid? movementId = null)
    {
        if (Status != CashRegisterStatus.Open)
        {
            return Result.Failure<Guid>(ErrorCodes.CashRegister.MovementNotAllowed);
        }

        var expected = ComputeExpectedCash(cashNet);
        if (amount > expected)
        {
            return Result.Failure<Guid>(ErrorCodes.CashRegister.InsufficientCash);
        }

        var movementResult = CashMovement.Create(Id, CashMovementKind.Drop, amount, reason, id: movementId);
        if (movementResult.IsFailure)
        {
            return Result.Failure<Guid>(movementResult.Error);
        }

        _movements.Add(movementResult.Value);
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success(movementResult.Value.Id);
    }

    /// <summary>Records a suprimento (cash added to drawer).</summary>
    public Result<Guid> RecordSupply(decimal amount, string reason, Guid? movementId = null)
    {
        if (Status != CashRegisterStatus.Open)
        {
            return Result.Failure<Guid>(ErrorCodes.CashRegister.MovementNotAllowed);
        }

        var movementResult = CashMovement.Create(Id, CashMovementKind.Supply, amount, reason, id: movementId);
        if (movementResult.IsFailure)
        {
            return Result.Failure<Guid>(movementResult.Error);
        }

        _movements.Add(movementResult.Value);
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success(movementResult.Value.Id);
    }

    /// <summary>
    /// Closes the session with counted cash and snapshots expected balance from sales/movements.
    /// </summary>
    public Result<bool> Close(decimal actualClosingBalance, decimal cashNet)
    {
        if (Status == CashRegisterStatus.Closed)
        {
            return Result.Failure<bool>(ErrorCodes.CashRegister.AlreadyClosed);
        }

        var expected = ComputeExpectedCash(cashNet);
        var expectedMoney = Money.Create(expected);
        if (expectedMoney.IsFailure)
        {
            return Result.Failure<bool>(expectedMoney.Error);
        }

        var moneyResult = Money.Create(actualClosingBalance);
        if (!moneyResult.IsSuccess)
        {
            return Result.Failure<bool>(moneyResult.Error);
        }

        ExpectedClosingBalance = expectedMoney.Value;
        ClosingBalance = moneyResult.Value;
        ClosedAt = DateTimeOffset.UtcNow;
        Status = CashRegisterStatus.Closed;
        UpdatedAt = DateTimeOffset.UtcNow;

        return Result.Success(true);
    }

    /// <summary>Variance after close (counted minus expected).</summary>
    public decimal? ClosingVariance =>
        Status == CashRegisterStatus.Closed
            ? ClosingBalance.Amount - ExpectedClosingBalance.Amount
            : null;

    /// <summary>Rehydrates a cash register from sync pull.</summary>
    public static CashRegister RestoreFromSync(
        Guid id,
        Guid openedByUserId,
        DateTimeOffset openedAt,
        DateTimeOffset? closedAt,
        decimal openingBalance,
        decimal expectedClosingBalance,
        decimal closingBalance,
        CashRegisterStatus status,
        DateTimeOffset updatedAt,
        IEnumerable<CashMovement>? movements = null)
    {
        var opening = Money.CreateUnsafe(openingBalance);
        var register = new CashRegister(id, openedByUserId, opening)
        {
            OpenedAt = openedAt,
            ClosedAt = closedAt,
            ExpectedClosingBalance = Money.CreateUnsafe(expectedClosingBalance),
            ClosingBalance = Money.CreateUnsafe(closingBalance),
            Status = status,
            UpdatedAt = updatedAt
        };

        if (movements != null)
        {
            register._movements.AddRange(movements);
        }

        return register;
    }
}
