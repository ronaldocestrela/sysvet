using Core.Domain;

namespace Platform.Domain.Entities;

/// <summary>Asaas customer profile for a SaaS tenant (dbo).</summary>
public sealed class BillingCustomer : Entity
{
    /// <summary>Owning tenant.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Billing contact name.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Billing e-mail.</summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>CPF or CNPJ digits only.</summary>
    public string CpfCnpj { get; private set; } = string.Empty;

    /// <summary>Asaas customer id (cus_...).</summary>
    public string? GatewayCustomerId { get; private set; }

#pragma warning disable CS8618
    private BillingCustomer()
    {
    }
#pragma warning restore CS8618

    /// <summary>Creates or updates local billing profile before gateway sync.</summary>
    public static Result<BillingCustomer> Create(Guid tenantId, string name, string email, string cpfCnpj)
    {
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<BillingCustomer>(ErrorCodes.Billing.InvalidTenant);
        }

        var normalizedName = name?.Trim() ?? string.Empty;
        var normalizedEmail = email?.Trim() ?? string.Empty;
        var digits = new string((cpfCnpj ?? string.Empty).Where(char.IsDigit).ToArray());
        if (normalizedName.Length < 2 || normalizedEmail.Length < 5 || digits.Length is not (11 or 14))
        {
            return Result.Failure<BillingCustomer>(ErrorCodes.Billing.InvalidCustomer);
        }

        return Result.Success(new BillingCustomer
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = normalizedName,
            Email = normalizedEmail,
            CpfCnpj = digits,
            UpdatedAt = DateTimeOffset.UtcNow,
            RowVersion = new byte[8]
        });
    }

    /// <summary>Updates profile fields.</summary>
    public Result Update(string name, string email, string cpfCnpj)
    {
        var created = Create(TenantId, name, email, cpfCnpj);
        if (created.IsFailure)
        {
            return created;
        }

        Name = created.Value.Name;
        Email = created.Value.Email;
        CpfCnpj = created.Value.CpfCnpj;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>Stores gateway customer id after create/update call.</summary>
    public void SetGatewayCustomerId(string gatewayCustomerId)
    {
        GatewayCustomerId = gatewayCustomerId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
