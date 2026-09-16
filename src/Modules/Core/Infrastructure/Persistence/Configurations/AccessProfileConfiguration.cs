using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF mapping for <see cref="AccessProfile"/> including JSON permission storage.
/// </summary>
public class AccessProfileConfiguration : IEntityTypeConfiguration<AccessProfile>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AccessProfile> builder)
    {
        builder.ToTable("AccessProfiles");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex("TenantId", nameof(AccessProfile.Name)).IsUnique();

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.BaseRole)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(p => p.IsSystem)
            .IsRequired();

        builder.Property(p => p.PermissionCodesStorage)
            .HasColumnName("PermissionCodesJson")
            .IsRequired();

        builder.Property(p => p.RowVersion)
            .IsConcurrencyToken();
    }
}
