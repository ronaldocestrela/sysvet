using System.ComponentModel.DataAnnotations;

namespace Core.Infrastructure.Configuration;

/// <summary>
/// Describes how module DbContexts resolve connection strings and which EF Core provider to use.
/// </summary>
public class DatabaseOptions
{
    /// <summary>
    /// Configuration section name for <see cref="DatabaseOptions"/>.
    /// </summary>
    public const string SectionName = "Database";

    /// <summary>
    /// EF Core provider identifier: <c>Sqlite</c> or <c>SqlServer</c>.
    /// </summary>
    [Required]
    public string Provider { get; set; } = "Sqlite";

    /// <summary>
    /// Name of the entry under <c>ConnectionStrings</c> used when a module does not override the connection.
    /// </summary>
    [Required]
    public string ConnectionStringName { get; set; } = "DefaultConnection";
}
