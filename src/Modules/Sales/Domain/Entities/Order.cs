using Core.Domain;
using Sales.Domain.Enums;
using Sales.Domain.ValueObjects;

namespace Sales.Domain.Entities;

/// <summary>
/// POS sales order aggregate (cart → payment → stock/finance side effects).
/// </summary>
public class Order : AggregateRoot
{
    public Guid CashRegisterId { get; private set; }
    public OrderStatus Status { get; private set; } = OrderStatus.Draft;
    public Guid? TutorId { get; private set; }
    public Guid? PetId { get; private set; }
    public Guid? SourceQuoteId { get; private set; }
    public FinanceIntegrationStatus FinanceIntegrationStatus { get; private set; } = FinanceIntegrationStatus.None;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? PaidAt { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private readonly List<Payment> _payments = new();
    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

    public Money TotalAmount => Money.CreateUnsafe(_items.Sum(i => i.TotalPrice.Amount));

    private Order() { }

    private Order(Guid id, Guid cashRegisterId, Guid? tutorId, Guid? petId, Guid? sourceQuoteId)
        : base(id)
    {
        CashRegisterId = cashRegisterId;
        TutorId = tutorId;
        PetId = petId;
        SourceQuoteId = sourceQuoteId;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Starts a draft order for an open cash register session (server-generated id).
    /// </summary>
    public static Result<Order> Create(
        Guid cashRegisterId,
        Guid? tutorId = null,
        Guid? petId = null,
        Guid? sourceQuoteId = null)
        => Create(Guid.NewGuid(), cashRegisterId, tutorId, petId, sourceQuoteId);

    /// <summary>
    /// Starts a draft order with a client-assigned id for offline sync (ADR-026).
    /// </summary>
    public static Result<Order> Create(
        Guid id,
        Guid cashRegisterId,
        Guid? tutorId = null,
        Guid? petId = null,
        Guid? sourceQuoteId = null)
    {
        if (id == Guid.Empty)
        {
            return Result.Failure<Order>(ErrorCodes.Order.InvalidId);
        }

        if (cashRegisterId == Guid.Empty)
        {
            return Result.Failure<Order>(ErrorCodes.Order.InvalidCashRegister);
        }

        if (petId.HasValue && petId != Guid.Empty && (!tutorId.HasValue || tutorId == Guid.Empty))
        {
            return Result.Failure<Order>(ErrorCodes.Order.PetRequiresTutor);
        }

        return Result.Success(new Order(id, cashRegisterId, tutorId, petId, sourceQuoteId));
    }

    /// <summary>
    /// Adds a product line that will consume inventory when paid.
    /// </summary>
    public Result<bool> AddProductItem(Guid productId, string productName, decimal quantity, decimal unitPrice)
    {
        if (productId == Guid.Empty)
        {
            return Result.Failure<bool>(ErrorCodes.OrderItem.ProductIdRequired);
        }

        return AddItemCore(OrderItemKind.Product, productId, productName, quantity, unitPrice);
    }

    /// <summary>
    /// Adds a service line (no stock movement).
    /// </summary>
    public Result<bool> AddServiceItem(string description, decimal quantity, decimal unitPrice)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return Result.Failure<bool>(ErrorCodes.OrderItem.DescriptionRequired);
        }

        return AddItemCore(OrderItemKind.Service, null, description.Trim(), quantity, unitPrice);
    }

    /// <summary>
    /// Legacy entry point for product lines (kept for backward compatibility in handlers).
    /// </summary>
    public Result<bool> AddItem(Guid productId, string productName, decimal quantity, decimal unitPrice)
        => AddProductItem(productId, productName, quantity, unitPrice);

    private Result<bool> AddItemCore(
        OrderItemKind kind,
        Guid? productId,
        string productName,
        decimal quantity,
        decimal unitPrice)
    {
        if (Status != OrderStatus.Draft)
        {
            return Result.Failure<bool>(ErrorCodes.Order.NotDraft);
        }

        if (quantity <= 0)
        {
            return Result.Failure<bool>(ErrorCodes.Order.InvalidQuantity);
        }

        if (unitPrice < 0)
        {
            return Result.Failure<bool>(ErrorCodes.Money.InvalidAmount);
        }

        _items.Add(new OrderItem(Id, kind, productId, productName, quantity, unitPrice));
        return Result.Success(true);
    }

    /// <summary>
    /// Completes payment when split amounts match order total.
    /// </summary>
    public Result<bool> Pay(IReadOnlyList<Payment> payments)
    {
        if (Status != OrderStatus.Draft && Status != OrderStatus.PendingPayment)
        {
            return Result.Failure<bool>(ErrorCodes.Order.InvalidStatus);
        }

        if (!_items.Any())
        {
            return Result.Failure<bool>(ErrorCodes.Order.Empty);
        }

        if (payments.Count == 0)
        {
            return Result.Failure<bool>(ErrorCodes.Payment.Required);
        }

        var paymentTotal = payments.Sum(p => p.Amount.Amount);
        if (paymentTotal != TotalAmount.Amount)
        {
            return Result.Failure<bool>(ErrorCodes.Order.PaymentTotalMismatch);
        }

        _payments.Clear();
        foreach (var payment in payments)
        {
            _payments.Add(Payment.Attach(Id, payment.Method, payment.Amount));
        }

        Status = OrderStatus.Paid;
        PaidAt = DateTimeOffset.UtcNow;
        FinanceIntegrationStatus = FinanceIntegrationStatus.Pending;
        UpdatedAt = DateTimeOffset.UtcNow;

        return Result.Success(true);
    }

    /// <summary>Rehydrates an order from sync pull (client mirror).</summary>
    public static Order RestoreFromSync(
        Guid id,
        Guid cashRegisterId,
        OrderStatus status,
        Guid? tutorId,
        Guid? petId,
        Guid? sourceQuoteId,
        FinanceIntegrationStatus financeIntegrationStatus,
        DateTimeOffset createdAt,
        DateTimeOffset? paidAt,
        DateTimeOffset updatedAt,
        IEnumerable<(Guid ItemId, OrderItemKind Kind, Guid? ProductId, string ProductName, decimal Quantity, decimal UnitPrice)> items,
        IEnumerable<(Guid PaymentId, PaymentMethod Method, decimal Amount)> payments)
    {
        var order = new Order(id, cashRegisterId, tutorId, petId, sourceQuoteId)
        {
            Status = status,
            FinanceIntegrationStatus = financeIntegrationStatus,
            CreatedAt = createdAt,
            PaidAt = paidAt,
            UpdatedAt = updatedAt
        };

        foreach (var item in items)
        {
            order._items.Add(OrderItem.Restore(item.ItemId, id, item.Kind, item.ProductId, item.ProductName, item.Quantity, item.UnitPrice));
        }

        foreach (var payment in payments)
        {
            order._payments.Add(Payment.Restore(payment.PaymentId, id, payment.Method, payment.Amount));
        }

        return order;
    }

}
