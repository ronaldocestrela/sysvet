using Core.Domain;
using Platform.Domain.ValueObjects;

namespace Platform.Domain.Entities;

/// <summary>
/// SaaS tenant registry row stored in schema <c>dbo</c> (ADR-003 catalog; lifecycle ADR-047).
/// </summary>
public sealed class Tenant : Entity
{
    /// <summary>Public slug for host/header resolution.</summary>
    public string Slug { get; private set; } = string.Empty;

    /// <summary>Display name shown in Super Admin UI.</summary>
    public string DisplayName { get; private set; } = string.Empty;

    /// <summary>SQL schema name for tenant data isolation (ADR-003).</summary>
    public string SchemaName { get; private set; } = string.Empty;

    /// <summary>Account lifecycle status.</summary>
    public TenantStatus Status { get; private set; } = TenantStatus.Active;

    /// <summary>When set, the tenant is soft-deleted and excluded from slug uniqueness checks.</summary>
    public DateTimeOffset? DeletedAt { get; private set; }

    /// <summary>When subscription was cancelled; used for logo churn (10.2).</summary>
    public DateTimeOffset? CancelledAt { get; private set; }

#pragma warning disable CS8618
    private Tenant()
    {
    }
#pragma warning restore CS8618

    /// <summary>
    /// Creates an active tenant with a stable schema name derived from <paramref name="id"/>.
    /// </summary>
    public static Result<Tenant> Create(Guid id, string slugRaw, string displayNameRaw)
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

        var displayName = displayNameRaw?.Trim() ?? string.Empty;
        if (displayName.Length < 2)
        {
            return Result.Failure<Tenant>(ErrorCodes.Tenant.InvalidDisplayName);
        }

        return Result.Success(new Tenant
        {
            Id = id,
            Slug = slugResult.Value,
            DisplayName = displayName,
            SchemaName = TenantSchema.FromId(id),
            Status = TenantStatus.Active,
            UpdatedAt = DateTimeOffset.UtcNow,
            RowVersion = new byte[8]
        });
    }

    /// <summary>Legacy factory for dev seed (display name derived from slug).</summary>
    public static Result<Tenant> Create(Guid id, string slugRaw) =>
        Create(id, slugRaw, slugRaw.Replace('-', ' '));

    /// <summary>Suspends an active tenant.</summary>
    public Result Suspend() => TransitionTo(TenantStatus.Suspended, TenantStatus.Active);

    /// <summary>Reactivates a suspended tenant.</summary>
    public Result Reactivate() => TransitionTo(TenantStatus.Active, TenantStatus.Suspended);

    /// <summary>Marks subscription cancelled.</summary>
    public Result Cancel() =>
        Status is TenantStatus.Active or TenantStatus.Suspended
            ? SetStatus(TenantStatus.Cancelled)
            : Result.Failure(ErrorCodes.Tenant.InvalidStatusTransition);

    /// <summary>Soft-deletes the tenant catalog row.</summary>
    public Result MarkDeleted()
    {
        if (Status == TenantStatus.Deleted)
        {
            return Result.Failure(ErrorCodes.Tenant.AlreadyDeleted);
        }

        DeletedAt = DateTimeOffset.UtcNow;
        return SetStatus(TenantStatus.Deleted);
    }

    /// <summary>Applies a new status from Super Admin commands.</summary>
    public Result ChangeStatus(TenantStatus target)
    {
        if (Status == TenantStatus.Deleted)
        {
            return Result.Failure(ErrorCodes.Tenant.AlreadyDeleted);
        }

        return target switch
        {
            TenantStatus.Active when Status is TenantStatus.Suspended => Reactivate(),
            TenantStatus.Suspended when Status is TenantStatus.Active => Suspend(),
            TenantStatus.Cancelled when Status is TenantStatus.Active or TenantStatus.Suspended => Cancel(),
            TenantStatus.Deleted => MarkDeleted(),
            _ when target == Status => Result.Success(),
            _ => Result.Failure(ErrorCodes.Tenant.InvalidStatusTransition)
        };
    }

    private Result TransitionTo(TenantStatus target, TenantStatus requiredCurrent)
    {
        if (Status == TenantStatus.Deleted)
        {
            return Result.Failure(ErrorCodes.Tenant.AlreadyDeleted);
        }

        if (Status != requiredCurrent)
        {
            return Result.Failure(ErrorCodes.Tenant.InvalidStatusTransition);
        }

        return SetStatus(target);
    }

    private Result SetStatus(TenantStatus status)
    {
        if (status == TenantStatus.Cancelled && CancelledAt is null)
        {
            CancelledAt = DateTimeOffset.UtcNow;
        }

        Status = status;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }
}
