using System.ComponentModel.DataAnnotations;

namespace Core.Infrastructure.Tenancy;

public class TenancySettings
{
    public const string SectionName = "TenancySettings";

    [Required]
    public string DefaultSchema { get; set; } = "dbo";
}
