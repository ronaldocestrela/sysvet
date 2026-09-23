using Core.Domain;

namespace Platform.Domain.Entities;

/// <summary>Preferred payment rail for tenant SaaS billing (dbo).</summary>
public sealed class BillingPaymentMethod : Entity
{
    /// <summary>Owning tenant.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Active payment kind.</summary>
    public BillingPaymentMethodKind Kind { get; private set; }

    /// <summary>Asaas credit card token when kind is credit card.</summary>
    public string? CreditCardToken { get; private set; }

#pragma warning disable CS8618
    private BillingPaymentMethod()
    {
    }
#pragma warning restore CS8618

    /// <summary>Sets payment method for tenant.</summary>
    public static Result<BillingPaymentMethod> Create(
        Guid tenantId,
        BillingPaymentMethodKind kind,
        string? creditCardToken)
    {
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<BillingPaymentMethod>(ErrorCodes.Billing.InvalidTenant);
        }

        if (kind == BillingPaymentMethodKind.CreditCard && string.IsNullOrWhiteSpace(creditCardToken))
        {
            return Result.Failure<BillingPaymentMethod>(ErrorCodes.Billing.InvalidPaymentMethod);
        }

        return Result.Success(new BillingPaymentMethod
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Kind = kind,
            CreditCardToken = kind == BillingPaymentMethodKind.CreditCard ? creditCardToken!.Trim() : null,
            UpdatedAt = DateTimeOffset.UtcNow,
            RowVersion = new byte[8]
        });
    }

    /// <summary>Updates kind and optional token.</summary>
    public Result Update(BillingPaymentMethodKind kind, string? creditCardToken)
    {
        var created = Create(TenantId, kind, creditCardToken);
        if (created.IsFailure)
        {
            return created;
        }

        Kind = kind;
        CreditCardToken = created.Value.CreditCardToken;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }
}
