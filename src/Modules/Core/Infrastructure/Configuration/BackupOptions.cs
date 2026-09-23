using System.ComponentModel.DataAnnotations;

namespace Core.Infrastructure.Configuration;

/// <summary>SQL Server backup retention settings referenced by drill scripts (10.5).</summary>
public sealed class BackupOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Backup";

    /// <summary>Days to retain backup files on disk.</summary>
    [Range(1, 365)]
    public int RetentionDays { get; set; } = 14;
}
