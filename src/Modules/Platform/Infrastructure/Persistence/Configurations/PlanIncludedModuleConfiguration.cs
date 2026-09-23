using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Domain.Entities;

namespace Platform.Infrastructure.Persistence.Configurations;

internal sealed class PlanIncludedModuleConfiguration : IEntityTypeConfiguration<PlanIncludedModule>
{
    public void Configure(EntityTypeBuilder<PlanIncludedModule> builder)
    {
        builder.ToTable("PlatformPlanModules");
        builder.HasKey(m => new { m.PlanId, m.Module });
        builder.Property(m => m.Module).HasConversion<int>();
    }
}
