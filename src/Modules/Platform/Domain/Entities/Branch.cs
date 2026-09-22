using Core.Domain;
using Platform.Domain.ValueObjects;

namespace Platform.Domain.Entities;

/// <summary>
/// Legal entity (CNPJ) linked to a tenant account; headquarters flag marks the matrix branch (ADR-047).
/// </summary>
public sealed class Branch : Entity
{
    /// <summary>Owning tenant id.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>CNPJ digits (14).</summary>
    public string Cnpj { get; private set; } = string.Empty;

    /// <summary>Registered legal name.</summary>
    public string LegalName { get; private set; } = string.Empty;

    /// <summary>When true, this branch is the tenant matrix (only one per tenant).</summary>
    public bool IsHeadquarters { get; private set; }

    /// <summary>Soft-delete timestamp when removed from the catalog.</summary>
    public DateTimeOffset? DeletedAt { get; private set; }

#pragma warning disable CS8618
    private Branch()
    {
    }
#pragma warning restore CS8618

    /// <summary>Creates the matrix branch for onboarding.</summary>
    public static Result<Branch> CreateHeadquarters(Guid tenantId, string cnpjRaw, string legalNameRaw) =>
        CreateInternal(tenantId, cnpjRaw, legalNameRaw, isHeadquarters: true);

    /// <summary>Creates a non-headquarters branch.</summary>
    public static Result<Branch> CreateBranch(Guid tenantId, string cnpjRaw, string legalNameRaw) =>
        CreateInternal(tenantId, cnpjRaw, legalNameRaw, isHeadquarters: false);

    /// <summary>Updates legal name for an active branch.</summary>
    public Result UpdateLegalName(string legalNameRaw)
    {
        if (DeletedAt is not null)
        {
            return Result.Failure(ErrorCodes.Branch.AlreadyDeleted);
        }

        var name = legalNameRaw?.Trim() ?? string.Empty;
        if (name.Length < 2)
        {
            return Result.Failure(ErrorCodes.Branch.InvalidLegalName);
        }

        LegalName = name;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>Soft-deletes the branch row.</summary>
    public Result MarkDeleted()
    {
        if (DeletedAt is not null)
        {
            return Result.Failure(ErrorCodes.Branch.AlreadyDeleted);
        }

        DeletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    private static Result<Branch> CreateInternal(Guid tenantId, string cnpjRaw, string legalNameRaw, bool isHeadquarters)
    {
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<Branch>(ErrorCodes.Branch.InvalidTenant);
        }

        var cnpjResult = BranchCnpj.Create(cnpjRaw);
        if (cnpjResult.IsFailure)
        {
            return Result.Failure<Branch>(cnpjResult.Error);
        }

        var name = legalNameRaw?.Trim() ?? string.Empty;
        if (name.Length < 2)
        {
            return Result.Failure<Branch>(ErrorCodes.Branch.InvalidLegalName);
        }

        return Result.Success(new Branch
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Cnpj = cnpjResult.Value.Value,
            LegalName = name,
            IsHeadquarters = isHeadquarters,
            UpdatedAt = DateTimeOffset.UtcNow,
            RowVersion = new byte[8]
        });
    }
}
