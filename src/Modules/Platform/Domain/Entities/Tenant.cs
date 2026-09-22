using Core.Domain;
using Platform.Domain.ValueObjects;

namespace Platform.Domain.Entities;

/// <summary>
/// SaaS tenant registry row stored in schema <c>dbo</c> (ADR-003 catalog; onboarding in 9.2).
/// </summary>
public sealed class Tenant : Entity
{
    /// <summary>Public slug for host/header resolution.</summary>
    public string Slug { get; private set; } = string.Empty;

    /// <summary>SQL schema name for tenant data isolation.</summary>
    public string SchemaName { get; private set; } = string.Empty;

#pragma warning disable CS8618
    private Tenant()
    {
    }
#pragma warning restore CS8618

    /// <summary>
    /// Creates a tenant with a stable schema name derived from <paramref name="id"/>.
    /// </summary>
    public static Result<Tenant> Create(Guid id, string slugRaw)
    {
        var slugResult = TenantSlug.Create(slugRaw);
        if (slugResult.IsFailure)
        {
            return Result.Failure<Tenant>(slugResult.Error);
        }

        if (id == Guid.Empty)
        {
            return Result.Failure<Tenant>(ErrorCodes.Tenant.InvalidId);
        }

        return Result.Success(new Tenant
        {
            Id = id,
            Slug = slugResult.Value,
            SchemaName = TenantSchema.FromId(id),
            UpdatedAt = DateTimeOffset.UtcNow,
            RowVersion = new byte[8]
        });
    }
}
