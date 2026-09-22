using Commerce.Domain.Enums;
using Commerce.Domain.ValueObjects;
using Core.Domain;

namespace Commerce.Domain.Entities;

/// <summary>
/// E-commerce order distinct from PDV Sales.Order (ADR-045).
/// </summary>
public sealed class OnlineOrder : AggregateRoot
{
    public OnlineOrderChannel Channel { get; private set; }
    public OnlineOrderStatus Status { get; private set; }
    public OnlineOrderFulfillment Fulfillment { get; private set; }
    public string BuyerName { get; private set; } = string.Empty;
    public string BuyerPhone { get; private set; } = string.Empty;
    public string? BuyerEmail { get; private set; }
    public Guid? TutorId { get; private set; }
    public string? ExternalOrderId { get; private set; }
    public DateTimeOffset? ConfirmedAt { get; private set; }

    private readonly List<OnlineOrderLine> _lines = new();

    /// <summary>Order lines with unit price snapshots.</summary>
    public IReadOnlyCollection<OnlineOrderLine> Lines => _lines.AsReadOnly();

    /// <summary>Total after line snapshots.</summary>
    public decimal TotalAmount => _lines.Sum(l => l.UnitPrice.Amount * l.Quantity);

#pragma warning disable CS8618
    private OnlineOrder() : base(Guid.Empty) { }
#pragma warning restore CS8618

    private OnlineOrder(
        Guid id,
        OnlineOrderChannel channel,
        OnlineOrderFulfillment fulfillment,
        string buyerName,
        string buyerPhone,
        string? buyerEmail,
        Guid? tutorId,
        string? externalOrderId)
        : base(id)
    {
        Channel = channel;
        Fulfillment = fulfillment;
        Status = OnlineOrderStatus.Placed;
        BuyerName = buyerName;
        BuyerPhone = buyerPhone;
        BuyerEmail = buyerEmail;
        TutorId = tutorId;
        ExternalOrderId = externalOrderId;
    }

    /// <summary>
    /// Creates a storefront pickup order before confirmation.
    /// </summary>
    public static Result<OnlineOrder> CreateStorePickup(
        string buyerName,
        string buyerPhone,
        string? buyerEmail,
        IReadOnlyList<(Guid ProductId, Guid OfferId, string Name, string Sku, decimal Qty, Money UnitPrice)> lines,
        Guid? tutorId = null)
    {
        if (string.IsNullOrWhiteSpace(buyerName) || string.IsNullOrWhiteSpace(buyerPhone))
        {
            return Result.Failure<OnlineOrder>(ErrorCodes.Order.BuyerRequired);
        }

        if (lines.Count == 0)
        {
            return Result.Failure<OnlineOrder>(ErrorCodes.Order.Empty);
        }

        var order = new OnlineOrder(
            Guid.NewGuid(),
            OnlineOrderChannel.Store,
            OnlineOrderFulfillment.PickupAtClinic,
            buyerName.Trim(),
            buyerPhone.Trim(),
            string.IsNullOrWhiteSpace(buyerEmail) ? null : buyerEmail.Trim(),
            tutorId,
            externalOrderId: null);

        foreach (var line in lines)
        {
            var add = OnlineOrderLine.Create(
                order.Id,
                line.ProductId,
                line.OfferId,
                line.Name,
                line.Sku,
                line.Qty,
                line.UnitPrice);
            if (add.IsFailure)
            {
                return Result.Failure<OnlineOrder>(add.Error);
            }

            order._lines.Add(add.Value);
        }

        return Result.Success(order);
    }

    /// <summary>
    /// Creates a Mercado Livre order stub before confirmation.
    /// </summary>
    public static Result<OnlineOrder> CreateMercadoLivre(
        string buyerName,
        string buyerPhone,
        string? buyerEmail,
        string externalOrderId,
        IReadOnlyList<(Guid ProductId, Guid OfferId, string Name, string Sku, decimal Qty, Money UnitPrice)> lines)
    {
        if (string.IsNullOrWhiteSpace(externalOrderId))
        {
            return Result.Failure<OnlineOrder>(ErrorCodes.Order.Empty);
        }

        var orderResult = CreateStorePickup(buyerName, buyerPhone, buyerEmail, lines);
        if (orderResult.IsFailure)
        {
            return orderResult;
        }

        var order = orderResult.Value;
        order.Channel = OnlineOrderChannel.MercadoLivre;
        order.Fulfillment = OnlineOrderFulfillment.MarketplaceExternal;
        order.ExternalOrderId = externalOrderId.Trim();
        return Result.Success(order);
    }

    /// <summary>
    /// Marks the order confirmed after stock debit succeeds in the application layer.
    /// </summary>
    public Result Confirm(DateTimeOffset? at = null)
    {
        if (Status != OnlineOrderStatus.Placed)
        {
            return Result.Failure(ErrorCodes.Order.InvalidTransition);
        }

        if (_lines.Count == 0)
        {
            return Result.Failure(ErrorCodes.Order.Empty);
        }

        Status = OnlineOrderStatus.Confirmed;
        ConfirmedAt = at ?? DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>Staff marks order ready for pickup.</summary>
    public Result MarkReadyForPickup()
    {
        if (Status != OnlineOrderStatus.Confirmed)
        {
            return Result.Failure(ErrorCodes.Order.InvalidTransition);
        }

        Status = OnlineOrderStatus.ReadyForPickup;
        return Result.Success();
    }

    /// <summary>Staff completes fulfillment.</summary>
    public Result Complete()
    {
        if (Status is not (OnlineOrderStatus.Confirmed or OnlineOrderStatus.ReadyForPickup))
        {
            return Result.Failure(ErrorCodes.Order.InvalidTransition);
        }

        Status = OnlineOrderStatus.Completed;
        return Result.Success();
    }

    /// <summary>Cancels a non-completed order.</summary>
    public Result Cancel()
    {
        if (Status == OnlineOrderStatus.Completed)
        {
            return Result.Failure(ErrorCodes.Order.InvalidTransition);
        }

        Status = OnlineOrderStatus.Cancelled;
        return Result.Success();
    }

    /// <summary>Whether stock should be restored on cancel.</summary>
    public bool RequiresStockRestore() =>
        Status == OnlineOrderStatus.Cancelled && ConfirmedAt.HasValue;
}
