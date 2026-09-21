using Finance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finance.Infrastructure.Persistence.Configurations;

internal sealed class TitleAllocationConfiguration : IEntityTypeConfiguration<TitleAllocation>
{
    public void Configure(EntityTypeBuilder<TitleAllocation> builder)
    {
        builder.ToTable("TitleAllocations");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Amount).HasPrecision(18, 2);
        builder.Property(a => a.Method).HasMaxLength(40).IsRequired();
        builder.Property(a => a.ExternalReference).HasMaxLength(64);
        builder.Property(a => a.Kind).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.HasIndex(a => new { a.FinancialTitleId, a.CorrelationId, a.Kind }).IsUnique();
        builder.HasIndex(a => a.ExternalReference);
    }
}
