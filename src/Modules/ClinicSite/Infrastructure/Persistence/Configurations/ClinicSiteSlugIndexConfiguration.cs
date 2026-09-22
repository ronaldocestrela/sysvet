using ClinicSite.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicSite.Infrastructure.Persistence.Configurations;

/// <summary>
/// Global slug index stored in the shared dbo schema (ADR-044).
/// </summary>
public sealed class ClinicSiteSlugIndexConfiguration : IEntityTypeConfiguration<ClinicSiteSlugIndex>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ClinicSiteSlugIndex> builder)
    {
        builder.ToTable("ClinicSiteSlugs", "dbo");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Slug).HasMaxLength(63).IsRequired();
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => x.TenantId).IsUnique();
    }
}
