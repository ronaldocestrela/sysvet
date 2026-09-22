using Automations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automations.Infrastructure.Persistence.Configurations;

internal sealed class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.ToTable("Campaigns");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.TemplateCode).HasMaxLength(100).IsRequired();
        builder.Property(c => c.SegmentKind).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.HasMany(c => c.Runs).WithOne().HasForeignKey(r => r.CampaignId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(c => c.Runs).HasField("_runs");
    }
}
