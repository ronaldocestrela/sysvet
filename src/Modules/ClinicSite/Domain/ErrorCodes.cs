using Core.Domain;

namespace ClinicSite.Domain;

/// <summary>
/// Standardized error codes for the ClinicSite module.
/// </summary>
public static class ErrorCodes
{
    /// <summary>
    /// Public site profile and publish rules.
    /// </summary>
    public static class Site
    {
        /// <summary>Site content was not found for the tenant.</summary>
        public static readonly Error NotFound = new("ClinicSite.NotFound", "Clinic site not found.");

        /// <summary>Site is not published for public access.</summary>
        public static readonly Error NotPublished = new("ClinicSite.NotPublished", "Clinic site is not published.");

        /// <summary>Slug format is invalid.</summary>
        public static readonly Error InvalidSlug = new("ClinicSite.InvalidSlug", "Site slug is invalid.");

        /// <summary>Another tenant already owns the slug.</summary>
        public static readonly Error SlugTaken = new("ClinicSite.SlugTaken", "This site address is already in use.");

        /// <summary>Publish prerequisites are missing.</summary>
        public static readonly Error PublishIncomplete = new("ClinicSite.PublishIncomplete", "Complete display name and contact before publishing.");

        /// <summary>Public slug lookup failed.</summary>
        public static readonly Error SlugNotFound = new("ClinicSite.SlugNotFound", "Site not found.");
    }
}
