using Core.Domain;
using Sales.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sales.Domain.Entities;

public class Order : AggregateRoot
{
    public Guid CashRegisterId { get; private set; }
    public string Status { get; private set; } = "Draft"; // Draft, PendingPayment, Paid, Cancelled
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? PaidAt { get; private set; }
    
    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public Money TotalAmount => Money.CreateUnsafe(_items.Sum(i => i.TotalPrice.Amount));

    private Order() { } // EF Core

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
            return Result.Failure<bool>(new Error("Order.NotDraft", "Não é possível adicionar itens a um pedido que não está em rascunho."));
        }

        if (quantity <= 0)
        {
            return Result.Failure<bool>(new Error("Order.InvalidQuantity", "A quantidade deve ser maior que zero."));
        }

        _items.Add(new OrderItem(Id, productId, productName, quantity, unitPrice));
        return Result.Success(true);
    }

    public Result<bool> Pay()
    {
        if (Status != "Draft" && Status != "PendingPayment")
        {
            return Result.Failure<bool>(new Error("Order.InvalidStatus", "Apenas pedidos em rascunho ou aguardando pagamento podem ser pagos."));
        }

        if (!_items.Any())
        {
            return Result.Failure<bool>(new Error("Order.Empty", "Não é possível pagar um pedido sem itens."));
        }

        Status = "Paid";
        PaidAt = DateTimeOffset.UtcNow;

        return Result.Success(true);
    }
}
