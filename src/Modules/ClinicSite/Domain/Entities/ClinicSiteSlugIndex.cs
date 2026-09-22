using Core.Domain;

namespace ClinicSite.Domain.Entities;

/// <summary>
/// Global slug registry mapping public subdomain to tenant (stored outside tenant schema).
/// </summary>
public sealed class ClinicSiteSlugIndex : Entity
{
    /// <summary>Normalized slug (unique globally).</summary>
    public string Slug { get; private set; } = string.Empty;

    /// <summary>Tenant that owns this slug.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Whether the site is currently reachable at the public URL.</summary>
    public bool IsPublished { get; private set; }

#pragma warning disable CS8618
    private ClinicSiteSlugIndex() : base(Guid.Empty) { }
#pragma warning restore CS8618

    private ClinicSiteSlugIndex(Guid id, string slug, Guid tenantId, bool isPublished)
        : base(id)
    {
        Slug = slug;
        TenantId = tenantId;
        IsPublished = isPublished;
    }

    /// <summary>
    /// Creates a new slug reservation for a tenant.
    /// </summary>
    public static Result<ClinicSiteSlugIndex> Create(string slug, Guid tenantId, bool isPublished = false, Guid id = default)
    {
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<ClinicSiteSlugIndex>(ErrorCodes.Site.InvalidSlug);
        }

        var normalized = ValueObjects.PublicSiteSlug.Normalize(slug);
        if (!ValueObjects.PublicSiteSlug.IsValid(normalized))
        {
            return Result.Failure<ClinicSiteSlugIndex>(ErrorCodes.Site.InvalidSlug);
        }

        return Result.Success(new ClinicSiteSlugIndex(id == Guid.Empty ? Guid.NewGuid() : id, normalized, tenantId, isPublished));
    }

    /// <summary>
    /// Marks the slug as published for anonymous visitors.
    /// </summary>
    public void MarkPublished() => IsPublished = true;

    /// <summary>
    /// Hides the site from public routes while keeping slug reserved.
    /// </summary>
    public void MarkUnpublished() => IsPublished = false;

    /// <summary>
    /// Changes slug text after validation (caller must ensure uniqueness).
    /// </summary>
    public Result UpdateSlug(string slug)
    {
        var normalized = ValueObjects.PublicSiteSlug.Normalize(slug);
        if (!ValueObjects.PublicSiteSlug.IsValid(normalized))
        {
            return Result.Failure(ErrorCodes.Site.InvalidSlug);
        }

        Slug = normalized;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }
}
