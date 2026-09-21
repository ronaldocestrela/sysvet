using Finance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finance.Infrastructure.Persistence.Configurations;

internal sealed class CardReconciliationBatchConfiguration : IEntityTypeConfiguration<CardReconciliationBatch>
{
    public void Configure(EntityTypeBuilder<CardReconciliationBatch> builder)
    {
        builder.ToTable("CardReconciliationBatches");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Reference).HasMaxLength(200).IsRequired();

        builder.HasMany(b => b.Lines)
            .WithOne()
            .HasForeignKey(l => l.BatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(b => b.Lines).HasField("_lines");
    }
}

internal sealed class CardReconciliationLineConfiguration : IEntityTypeConfiguration<CardReconciliationLine>
{
    public void Configure(EntityTypeBuilder<CardReconciliationLine> builder)
    {
        builder.ToTable("CardReconciliationLines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Nsu).HasMaxLength(64).IsRequired();
        builder.Property(l => l.Method).HasMaxLength(40);
        builder.Property(l => l.Amount).HasPrecision(18, 2);
        builder.Property(l => l.Fee).HasPrecision(18, 2);
        builder.Property(l => l.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
    }
}
