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
        builder.Property(b => b.RowVersion).IsConcurrencyToken();
        builder.HasIndex(b => new { b.TenantId, b.Cnpj }).IsUnique();
        builder.HasIndex(b => b.TenantId);
    }
}
