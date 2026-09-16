using Core.Domain;
using Sales.Domain.ValueObjects;

namespace Sales.Domain.Entities;

public class Order : AggregateRoot
{
    public Guid CashRegisterId { get; private set; }
    public string Status { get; private set; } = "Draft";
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? PaidAt { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public Money TotalAmount => Money.CreateUnsafe(_items.Sum(i => i.TotalPrice.Amount));

    private Order() { }

    private Order(Guid cashRegisterId)
    {
        CashRegisterId = cashRegisterId;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static Result<Order> Create(Guid cashRegisterId)
    {
        return Result.Success(new Order(cashRegisterId));
    }

    public Result<bool> AddItem(Guid productId, string productName, decimal quantity, decimal unitPrice)
    {
        if (Status != "Draft")
        {
            return Result.Failure<bool>(ErrorCodes.Order.NotDraft);
        }

        if (quantity <= 0)
        {
            return Result.Failure<bool>(ErrorCodes.Order.InvalidQuantity);
        }

        _items.Add(new OrderItem(Id, productId, productName, quantity, unitPrice));
        return Result.Success(true);
    }

    public Result<bool> Pay()
    {
        if (Status != "Draft" && Status != "PendingPayment")
        {
            return Result.Failure<bool>(ErrorCodes.Order.InvalidStatus);
        }

        if (!_items.Any())
        {
            return Result.Failure<bool>(ErrorCodes.Order.Empty);
        }

        Status = "Paid";
        PaidAt = DateTimeOffset.UtcNow;

        return Result.Success(true);
    }
}
