using System.ComponentModel.DataAnnotations;

namespace Core.Infrastructure.Configuration;

/// <summary>Configuration for clinical and module blob storage backends.</summary>
public class BlobStorageOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "BlobStorage";

    /// <summary>Provider identifier: <c>Local</c> or <c>Azure</c>.</summary>
    [Required]
    public string Provider { get; set; } = "Local";

    /// <summary>Root directory for <c>Local</c> provider (created on demand).</summary>
    public string LocalRootPath { get; set; } = "App_Data/blobs";

    /// <summary>Azure Storage connection string when <see cref="Provider"/> is Azure.</summary>
    public string? AzureConnectionString { get; set; }

    /// <summary>Azure blob container name.</summary>
    public string AzureContainer { get; set; } = "clinical";
}
