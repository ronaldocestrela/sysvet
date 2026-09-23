using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Domain.Entities;

namespace Platform.Infrastructure.Persistence.Configurations;

/// <summary>EF mapping for tenant branches.</summary>
internal sealed class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("PlatformBranches");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Cnpj).HasMaxLength(14).IsRequired();
        builder.Property(b => b.LegalName).HasMaxLength(256).IsRequired();
        builder.Property(b => b.PostalCode).HasMaxLength(8);
        builder.Property(b => b.Street).HasMaxLength(256);
        builder.Property(b => b.StreetNumber).HasMaxLength(32);
        builder.Property(b => b.District).HasMaxLength(128);
        builder.Property(b => b.City).HasMaxLength(128);
        builder.Property(b => b.StateCode).HasMaxLength(2);
        builder.Property(b => b.RowVersion).IsConcurrencyToken();
        builder.HasIndex(b => new { b.TenantId, b.Cnpj }).IsUnique();
        builder.HasIndex(b => b.TenantId);
    }
}
