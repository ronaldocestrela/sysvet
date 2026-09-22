namespace ClinicSiteWeb.Services;

/// <summary>
/// Holds the resolved public site slug for the current browser session.
/// </summary>
public sealed class SlugContext
{
    /// <summary>Normalized slug from subdomain or <c>/s/{slug}</c> route.</summary>
    public string? CurrentSlug { get; private set; }

    /// <summary>
    /// Sets the active slug when resolved from host or route.
    /// </summary>
    public void SetSlug(string slug) => CurrentSlug = slug.Trim().ToLowerInvariant();
}
