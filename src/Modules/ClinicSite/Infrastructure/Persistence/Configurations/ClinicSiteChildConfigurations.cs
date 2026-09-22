using ClinicSite.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicSite.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF mapping for clinic site service rows.
/// </summary>
public sealed class ClinicSiteServiceItemConfiguration : IEntityTypeConfiguration<ClinicSiteServiceItem>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ClinicSiteServiceItem> builder)
    {
        builder.ToTable("ClinicSiteServices");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.Price).HasPrecision(18, 2);
    }
}

/// <summary>
/// EF mapping for clinic site team rows.
/// </summary>
public sealed class ClinicSiteTeamMemberConfiguration : IEntityTypeConfiguration<ClinicSiteTeamMember>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ClinicSiteTeamMember> builder)
    {
        builder.ToTable("ClinicSiteTeamMembers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.RoleTitle).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Bio).HasMaxLength(2000);
    }
}

/// <summary>
/// EF mapping for clinic site opening hours rows.
/// </summary>
public sealed class ClinicSiteOpeningHoursConfiguration : IEntityTypeConfiguration<ClinicSiteOpeningHours>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ClinicSiteOpeningHours> builder)
    {
        builder.ToTable("ClinicSiteOpeningHours");
        builder.HasKey(x => x.Id);
    }
}
