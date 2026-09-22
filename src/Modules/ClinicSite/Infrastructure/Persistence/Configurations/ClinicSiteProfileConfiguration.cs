using ClinicSite.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicSite.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF mapping for the clinic site profile aggregate root.
/// </summary>
public sealed class ClinicSiteProfileConfiguration : IEntityTypeConfiguration<ClinicSiteProfile>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ClinicSiteProfile> builder)
    {
        builder.ToTable("ClinicSiteProfiles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Key).HasMaxLength(32).IsRequired();
        builder.HasIndex(x => x.Key).IsUnique();
        builder.Property(x => x.DisplayName).HasMaxLength(200);
        builder.Property(x => x.Tagline).HasMaxLength(300);
        builder.Property(x => x.Street).HasMaxLength(200);
        builder.Property(x => x.Number).HasMaxLength(20);
        builder.Property(x => x.Complement).HasMaxLength(100);
        builder.Property(x => x.District).HasMaxLength(100);
        builder.Property(x => x.City).HasMaxLength(100);
        builder.Property(x => x.State).HasMaxLength(2);
        builder.Property(x => x.PostalCode).HasMaxLength(8);
        builder.Property(x => x.Phone).HasMaxLength(30);
        builder.Property(x => x.Email).HasMaxLength(200);
        builder.Property(x => x.WhatsApp).HasMaxLength(30);
        builder.Property(x => x.LogoUrl).HasMaxLength(500);
        builder.Property(x => x.Slug).HasMaxLength(63);

        builder.Ignore(x => x.Services);
        builder.Ignore(x => x.Team);
        builder.Ignore(x => x.Hours);

        builder.HasMany<ClinicSiteServiceItem>("_services")
            .WithOne()
            .HasForeignKey(x => x.ClinicSiteProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<ClinicSiteTeamMember>("_team")
            .WithOne()
            .HasForeignKey(x => x.ClinicSiteProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<ClinicSiteOpeningHours>("_hours")
            .WithOne()
            .HasForeignKey(x => x.ClinicSiteProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation("_services").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation("_team").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation("_hours").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
