using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Domain.Entities;

namespace Platform.Infrastructure.Persistence.Configurations;

/// <summary>EF mapping for marketing acquisition spend (10.2).</summary>
internal sealed class AcquisitionSpendConfiguration : IEntityTypeConfiguration<AcquisitionSpend>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AcquisitionSpend> builder)
    {
        builder.ToTable("PlatformAcquisitionSpends");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Channel).HasMaxLength(64).IsRequired();
        builder.Property(s => s.Amount).HasPrecision(18, 2);
        builder.Property(s => s.Note).HasMaxLength(512);
        builder.HasIndex(s => new { s.Year, s.Month, s.Channel }).IsUnique();
        builder.Property(s => s.RowVersion).IsConcurrencyToken();
    }
}
