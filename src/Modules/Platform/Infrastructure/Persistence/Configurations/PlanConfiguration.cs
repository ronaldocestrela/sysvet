using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Domain.Entities;

namespace Platform.Infrastructure.Persistence.Configurations;

internal sealed class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("PlatformPlans");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Code).HasMaxLength(64).IsRequired();
        builder.Property(p => p.Name).HasMaxLength(256).IsRequired();
        builder.Property(p => p.MonthlyPrice).HasPrecision(18, 2);
        builder.HasIndex(p => p.Code).IsUnique();
        builder.Property(p => p.RowVersion).IsConcurrencyToken();
        builder.HasMany(p => p.IncludedModules).WithOne(m => m.Plan!).HasForeignKey(m => m.PlanId);
    }
}
