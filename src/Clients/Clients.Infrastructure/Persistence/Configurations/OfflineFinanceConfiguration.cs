using Finance.Domain.Entities;
using Finance.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clients.Infrastructure.Persistence.Configurations;

internal sealed class OfflineFinancialTitleConfiguration : IEntityTypeConfiguration<FinancialTitle>
{
    public void Configure(EntityTypeBuilder<FinancialTitle> builder)
    {
        builder.ToTable("FinancialTitles");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Direction).HasConversion<string>();
        builder.Property(t => t.Status).HasConversion<string>();
        builder.Property(t => t.SourceType).HasConversion<string>();
        builder.Property(t => t.PartyKind).HasConversion<string>();
        builder.Ignore(t => t.SettledAmount);
        builder.Ignore(t => t.OpenAmount);
        builder.HasMany(t => t.Allocations)
            .WithOne()
            .HasForeignKey(a => a.FinancialTitleId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(t => t.Allocations).HasField("_allocations");
    }
}

internal sealed class OfflineTitleAllocationConfiguration : IEntityTypeConfiguration<TitleAllocation>
{
    public void Configure(EntityTypeBuilder<TitleAllocation> builder)
    {
        builder.ToTable("TitleAllocations");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Kind).HasConversion<string>();
    }
}

internal sealed class OfflineFinancialCategoryConfiguration : IEntityTypeConfiguration<FinancialCategory>
{
    public void Configure(EntityTypeBuilder<FinancialCategory> builder)
    {
        builder.ToTable("FinancialCategories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Direction).HasConversion<string>();
    }
}

internal sealed class OfflineCostCenterConfiguration : IEntityTypeConfiguration<CostCenter>
{
    public void Configure(EntityTypeBuilder<CostCenter> builder)
    {
        builder.ToTable("CostCenters");
        builder.HasKey(c => c.Id);
    }
}
