using Core.Domain;
using Sales.Domain.Enums;
using Sales.Domain.Services;
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
    public Guid SellerUserId { get; private set; }
    public decimal DiscountPercent { get; private set; }
    public FinanceIntegrationStatus FinanceIntegrationStatus { get; private set; } = FinanceIntegrationStatus.None;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? PaidAt { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private readonly List<Payment> _payments = new();
    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

    private readonly List<SaleReturn> _returns = new();
    public IReadOnlyCollection<SaleReturn> Returns => _returns.AsReadOnly();

    private readonly List<CommissionAccrual> _commissions = new();
    public IReadOnlyCollection<CommissionAccrual> Commissions => _commissions.AsReadOnly();

    public decimal SubtotalAmount => _items.Sum(i => i.TotalPrice.Amount);

    public decimal DiscountAmount => OrderPricing.ComputeDiscountAmount(SubtotalAmount, DiscountPercent);

    public Money TotalAmount => Money.CreateUnsafe(SubtotalAmount - DiscountAmount);

    private Order() { }

    private Order(Guid id, Guid cashRegisterId, Guid? tutorId, Guid? petId, Guid? sourceQuoteId, Guid sellerUserId)
        : base(id)
    {
        CashRegisterId = cashRegisterId;
        TutorId = tutorId;
        PetId = petId;
        SourceQuoteId = sourceQuoteId;
        SellerUserId = sellerUserId;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Starts a draft order for an open cash register session (server-generated id).
    /// </summary>
    public static Result<Order> Create(
        Guid cashRegisterId,
        Guid sellerUserId,
        Guid? tutorId = null,
        Guid? petId = null,
        Guid? sourceQuoteId = null)
        => Create(Guid.NewGuid(), cashRegisterId, sellerUserId, tutorId, petId, sourceQuoteId);

    /// <summary>
    /// Starts a draft order with a client-assigned id for offline sync (ADR-026).
    /// </summary>
    public static Result<Order> Create(
        Guid id,
        Guid cashRegisterId,
        Guid sellerUserId,
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

        if (sellerUserId == Guid.Empty)
        {
            return Result.Failure<Order>(ErrorCodes.Order.InvalidSeller);
        }

        if (petId.HasValue && petId != Guid.Empty && (!tutorId.HasValue || tutorId == Guid.Empty))
        {
            return Result.Failure<Order>(ErrorCodes.Order.PetRequiresTutor);
        }

        return Result.Success(new Order(id, cashRegisterId, tutorId, petId, sourceQuoteId, sellerUserId));
    }

    /// <summary>Applies order-level discount percent while still in draft.</summary>
    public Result<bool> ApplyDiscount(decimal discountPercent)
    {
        if (Status != OrderStatus.Draft)
        {
            return Result.Failure<bool>(ErrorCodes.Order.NotDraft);
        }

        if (discountPercent < 0 || discountPercent > 100)
        {
            return Result.Failure<bool>(ErrorCodes.Order.InvalidDiscountPercent);
        }

        DiscountPercent = discountPercent;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success(true);
    }

    /// <summary>
    /// Adds a product line that will consume inventory when paid.
    /// </summary>
    public Result<bool> AddProductItem(Guid productId, string productName, decimal quantity, decimal unitPrice)
        => AddProductItem(productId, productName, quantity, unitPrice, null, null);

    /// <summary>
    /// Adds a product line with optional performer metadata.
    /// </summary>
    public Result<bool> AddProductItem(
        Guid productId,
        string productName,
        decimal quantity,
        decimal unitPrice,
        Guid? performerUserId,
        CommissionRole? performerRole)
    {
        if (productId == Guid.Empty)
        {
            return Result.Failure<bool>(ErrorCodes.OrderItem.ProductIdRequired);
        }

        return AddItemCore(OrderItemKind.Product, productId, productName, quantity, unitPrice, performerUserId, performerRole);
    }

    /// <summary>
    /// Adds a service line (no stock movement).
    /// </summary>
    public Result<bool> AddServiceItem(string description, decimal quantity, decimal unitPrice)
        => AddServiceItem(description, quantity, unitPrice, null, null);

    /// <summary>
    /// Adds a service line with optional performer (vet/groomer).
    /// </summary>
    public Result<bool> AddServiceItem(
        string description,
        decimal quantity,
        decimal unitPrice,
        Guid? performerUserId,
        CommissionRole? performerRole)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return Result.Failure<bool>(ErrorCodes.OrderItem.DescriptionRequired);
        }

        return AddItemCore(OrderItemKind.Service, null, description.Trim(), quantity, unitPrice, performerUserId, performerRole);
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
        decimal unitPrice,
        Guid? performerUserId,
        CommissionRole? performerRole)
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

        _items.Add(new OrderItem(Id, kind, productId, productName, quantity, unitPrice, performerUserId, performerRole));
        return Result.Success(true);
    }

    /// <summary>
    /// Attaches commission snapshots calculated at pay time.
    /// </summary>
    public void AttachCommissions(IEnumerable<CommissionAccrual> accruals)
    {
        _commissions.Clear();
        _commissions.AddRange(accruals);
    }

    /// <summary>
    /// Completes payment when split amounts match order total (after discount).
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
            _payments.Add(Payment.Attach(
                Id,
                payment.Method,
                payment.Amount,
                payment.Nsu,
                payment.AuthorizationCode,
                payment.Provider,
                payment.TerminalId,
                payment.Brand,
                payment.Installments));
        }

        Status = OrderStatus.Paid;
        PaidAt = DateTimeOffset.UtcNow;
        FinanceIntegrationStatus = FinanceIntegrationStatus.Pending;
        UpdatedAt = DateTimeOffset.UtcNow;

        return Result.Success(true);
    }

    /// <summary>
    /// Records a partial or full refund against a payment line (estorno — does not restore stock).
    /// </summary>
    public Result<PaymentRefund> RefundPayment(Guid paymentId, decimal amount, string? refundNsu = null)
    {
        if (Status is not (OrderStatus.Paid or OrderStatus.PartiallyRefunded or OrderStatus.PartiallyReturned
            or OrderStatus.Returned))
        {
            return Result.Failure<PaymentRefund>(ErrorCodes.Payment.RefundNotAllowed);
        }

        var payment = _payments.FirstOrDefault(p => p.Id == paymentId);
        if (payment is null)
        {
            return Result.Failure<PaymentRefund>(ErrorCodes.Payment.NotFound);
        }

        var refundResult = payment.ApplyRefund(amount, refundNsu);
        if (refundResult.IsFailure)
        {
            return refundResult;
        }

        UpdatedAt = DateTimeOffset.UtcNow;
        RefreshRefundStatus();

        return refundResult;
    }

    /// <summary>
    /// Returns sold items, computing net refund and reversing commissions proportionally.
    /// </summary>
    public Result<SaleReturn> ReturnItems(
        Guid returnId,
        IReadOnlyList<(Guid OrderItemId, decimal Quantity)> lines)
    {
        if (Status is not (OrderStatus.Paid or OrderStatus.PartiallyRefunded or OrderStatus.PartiallyReturned))
        {
            return Result.Failure<SaleReturn>(ErrorCodes.Order.ReturnNotAllowed);
        }

        if (lines.Count == 0)
        {
            return Result.Failure<SaleReturn>(ErrorCodes.Order.ReturnEmpty);
        }

        if (returnId == Guid.Empty)
        {
            return Result.Failure<SaleReturn>(ErrorCodes.Order.InvalidId);
        }

        if (_returns.Any(r => r.Id == returnId))
        {
            return Result.Success(_returns.First(r => r.Id == returnId));
        }

        var subtotal = SubtotalAmount;
        var netTotal = TotalAmount.Amount;
        decimal returnedGross = 0m;
        var returnLines = new List<(Guid LineId, Guid OrderItemId, decimal Quantity)>();

        foreach (var (orderItemId, quantity) in lines)
        {
            var item = _items.FirstOrDefault(i => i.Id == orderItemId);
            if (item is null)
            {
                return Result.Failure<SaleReturn>(ErrorCodes.Order.ReturnItemNotFound);
            }

            var record = item.RecordReturn(quantity);
            if (record.IsFailure)
            {
                return Result.Failure<SaleReturn>(record.Error);
            }

            returnedGross += item.UnitPrice.Amount * quantity;
            returnLines.Add((Guid.NewGuid(), orderItemId, quantity));

            ReverseCommissionsForItem(item, quantity);
        }

        var refundAmount = subtotal <= 0
            ? 0m
            : Math.Round(returnedGross / subtotal * netTotal, 2, MidpointRounding.AwayFromZero);

        var saleReturn = SaleReturn.Create(returnId, Id, refundAmount, returnLines);
        _returns.Add(saleReturn);
        UpdatedAt = DateTimeOffset.UtcNow;
        RefreshReturnStatus();

        return Result.Success(saleReturn);
    }

    private void ReverseCommissionsForItem(OrderItem item, decimal returnedQuantity)
    {
        if (item.Quantity <= 0)
        {
            return;
        }

        var ratio = returnedQuantity / item.Quantity;
        foreach (var accrual in _commissions.Where(c => c.OrderItemId == item.Id && c.Status == CommissionAccrualStatus.Accrued))
        {
            var reverseAmount = Math.Round(accrual.CommissionAmount.Amount * ratio, 2, MidpointRounding.AwayFromZero);
            accrual.Reverse(reverseAmount);
        }
    }

    private void RefreshReturnStatus()
    {
        var allReturned = _items.All(i => i.RemainingQuantity <= 0);
        Status = allReturned ? OrderStatus.Returned : OrderStatus.PartiallyReturned;
    }

    private void RefreshRefundStatus()
    {
        if (Status is OrderStatus.PartiallyReturned or OrderStatus.Returned)
        {
            return;
        }

        Status = _payments.All(p => p.RemainingRefundable == 0)
            ? OrderStatus.Refunded
            : OrderStatus.PartiallyRefunded;
    }

    /// <summary>Rehydrates an order from sync pull (client mirror).</summary>
    public static Order RestoreFromSync(
        Guid id,
        Guid cashRegisterId,
        OrderStatus status,
        Guid? tutorId,
        Guid? petId,
        Guid? sourceQuoteId,
        Guid sellerUserId,
        decimal discountPercent,
        FinanceIntegrationStatus financeIntegrationStatus,
        DateTimeOffset createdAt,
        DateTimeOffset? paidAt,
        DateTimeOffset updatedAt,
        IEnumerable<(Guid ItemId, OrderItemKind Kind, Guid? ProductId, string ProductName, decimal Quantity, decimal UnitPrice, Guid? PerformerUserId, CommissionRole? PerformerRole, decimal ReturnedQuantity)> items,
        IEnumerable<(
            Guid PaymentId,
            PaymentMethod Method,
            decimal Amount,
            string? Nsu,
            string? AuthorizationCode,
            string? Provider,
            string? TerminalId,
            string? Brand,
            int Installments,
            IEnumerable<(Guid RefundId, decimal RefundAmount, string? RefundNsu, DateTimeOffset CreatedAt)> Refunds)> payments,
        IEnumerable<(Guid AccrualId, Guid OrderItemId, Guid PayeeUserId, CommissionRole Role, decimal RatePercent, decimal BaseAmount, decimal CommissionAmount, CommissionAccrualStatus Status)> commissions,
        IEnumerable<(Guid ReturnId, decimal RefundAmount, DateTimeOffset CreatedAt, IEnumerable<(Guid LineId, Guid OrderItemId, decimal Quantity)> Lines)> returns)
    {
        var order = new Order(id, cashRegisterId, tutorId, petId, sourceQuoteId, sellerUserId)
        {
            Status = status,
            DiscountPercent = discountPercent,
            FinanceIntegrationStatus = financeIntegrationStatus,
            CreatedAt = createdAt,
            PaidAt = paidAt,
            UpdatedAt = updatedAt
        };

        foreach (var item in items)
        {
            order._items.Add(OrderItem.Restore(
                item.ItemId,
                id,
                item.Kind,
                item.ProductId,
                item.ProductName,
                item.Quantity,
                item.UnitPrice,
                item.PerformerUserId,
                item.PerformerRole,
                item.ReturnedQuantity));
        }

        foreach (var payment in payments)
        {
            order._payments.Add(Payment.Restore(
                payment.PaymentId,
                id,
                payment.Method,
                payment.Amount,
                payment.Nsu,
                payment.AuthorizationCode,
                payment.Provider,
                payment.TerminalId,
                payment.Brand,
                payment.Installments,
                payment.Refunds));
        }

        foreach (var accrual in commissions)
        {
            order._commissions.Add(CommissionAccrual.Restore(
                accrual.AccrualId,
                id,
                accrual.OrderItemId,
                accrual.PayeeUserId,
                accrual.Role,
                accrual.RatePercent,
                accrual.BaseAmount,
                accrual.CommissionAmount,
                accrual.Status));
        }

        foreach (var ret in returns)
        {
            order._returns.Add(SaleReturn.Restore(ret.ReturnId, id, ret.RefundAmount, ret.CreatedAt, ret.Lines));
        }

        return order;
    }
}
