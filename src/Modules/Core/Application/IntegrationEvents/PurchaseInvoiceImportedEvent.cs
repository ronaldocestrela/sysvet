using MediatR;

namespace Core.Application.IntegrationEvents;

/// <summary>
/// Published when a purchase NF-e import is confirmed; Finance 7.2 will create AP titles.
/// </summary>
public sealed class PurchaseInvoiceImportedEvent : INotification
{
    public Guid ImportId { get; }
    public string AccessKey { get; }
    public Guid SupplierId { get; }
    public decimal TotalAmount { get; }
    public IReadOnlyList<PurchaseInvoiceImportedDuplicate> Duplicates { get; }

    public PurchaseInvoiceImportedEvent(
        Guid importId,
        string accessKey,
        Guid supplierId,
        decimal totalAmount,
        IReadOnlyList<PurchaseInvoiceImportedDuplicate> duplicates)
    {
        ImportId = importId;
        AccessKey = accessKey;
        SupplierId = supplierId;
        TotalAmount = totalAmount;
        Duplicates = duplicates;
    }
}

/// <summary>Installment snapshot from NF-e cobr/dup for accounts payable.</summary>
public sealed class PurchaseInvoiceImportedDuplicate
{
    public string Number { get; }
    public DateOnly? DueDate { get; }
    public decimal Amount { get; }

    public PurchaseInvoiceImportedDuplicate(string number, DateOnly? dueDate, decimal amount)
    {
        Number = number;
        DueDate = dueDate;
        Amount = amount;
    }
}
