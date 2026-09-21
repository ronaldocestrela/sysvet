using Finance.Domain.Entities;
using Finance.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finance.Infrastructure.Persistence.Configurations;

internal sealed class FinancialTitleConfiguration : IEntityTypeConfiguration<FinancialTitle>
{
    public void Configure(EntityTypeBuilder<FinancialTitle> builder)
    {
        builder.ToTable("FinancialTitles");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Direction).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(t => t.SourceType).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(t => t.SourceInstallmentKey).HasMaxLength(40).IsRequired();
        builder.Property(t => t.PartyKind).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(500).IsRequired();
        builder.Property(t => t.OriginalAmount).HasPrecision(18, 2);
        builder.Property(t => t.IssueDate).IsRequired();
        builder.Property(t => t.DueDate).IsRequired();
        builder.Ignore(t => t.SettledAmount);
        builder.Ignore(t => t.OpenAmount);

        builder.HasIndex(t => new { t.SourceType, t.SourceId, t.SourceInstallmentKey })
            .IsUnique()
            .HasFilter($"[{nameof(FinancialTitle.SourceType)}] <> '{TitleSourceType.Manual}'");

        builder.HasMany(t => t.Allocations)
            .WithOne()
            .HasForeignKey(a => a.FinancialTitleId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(t => t.Allocations).HasField("_allocations");
    }
}
