using Core.Domain;
using Inventory.Domain;
using Inventory.Domain.ValueObjects;

namespace Inventory.Domain.Entities;

/// <summary>
/// Product supplier registered per tenant for purchases and catalog defaults.
/// </summary>
public class Supplier : AggregateRoot
{
    public string LegalName { get; private set; } = string.Empty;
    public string TradeName { get; private set; } = string.Empty;
    public string Document { get; private set; } = string.Empty;
    public string? ContactEmail { get; private set; }
    public string? ContactPhone { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Supplier() { }

    private Supplier(Guid id, string legalName, string tradeName, string document, string? contactEmail, string? contactPhone)
        : base(id)
    {
        LegalName = legalName;
        TradeName = tradeName;
        Document = document;
        ContactEmail = contactEmail;
        ContactPhone = contactPhone;
    }

    /// <summary>
    /// Creates a new supplier with validated CNPJ.
    /// </summary>
    public static Result<Supplier> Create(
        string legalName,
        string tradeName,
        string document,
        string? contactEmail,
        string? contactPhone,
        Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(legalName))
        {
            return Result.Failure<Supplier>(ErrorCodes.Supplier.InvalidLegalName);
        }

        var cnpjResult = Cnpj.Create(document);
        if (cnpjResult.IsFailure)
        {
            return Result.Failure<Supplier>(cnpjResult.Error);
        }

        var supplierId = id ?? Guid.NewGuid();
        return Result.Success(new Supplier(
            supplierId,
            legalName.Trim(),
            string.IsNullOrWhiteSpace(tradeName) ? legalName.Trim() : tradeName.Trim(),
            cnpjResult.Value.Value,
            string.IsNullOrWhiteSpace(contactEmail) ? null : contactEmail.Trim(),
            string.IsNullOrWhiteSpace(contactPhone) ? null : contactPhone.Trim()));
    }

    /// <summary>
    /// Updates supplier contact and display data (document immutable).
    /// </summary>
    public Result Update(string legalName, string tradeName, string? contactEmail, string? contactPhone)
    {
        if (string.IsNullOrWhiteSpace(legalName))
        {
            return Result.Failure(ErrorCodes.Supplier.InvalidLegalName);
        }

        LegalName = legalName.Trim();
        TradeName = string.IsNullOrWhiteSpace(tradeName) ? LegalName : tradeName.Trim();
        ContactEmail = string.IsNullOrWhiteSpace(contactEmail) ? null : contactEmail.Trim();
        ContactPhone = string.IsNullOrWhiteSpace(contactPhone) ? null : contactPhone.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>
    /// Soft-deactivates the supplier from new product links.
    /// </summary>
    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
