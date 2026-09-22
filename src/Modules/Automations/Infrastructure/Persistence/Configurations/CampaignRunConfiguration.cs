using Automations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automations.Infrastructure.Persistence.Configurations;

internal sealed class CampaignRunConfiguration : IEntityTypeConfiguration<CampaignRun>
{
    public void Configure(EntityTypeBuilder<CampaignRun> builder)
    {
        builder.ToTable("CampaignRuns");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.CampaignId).IsRequired();
        builder.HasIndex(r => r.CampaignId);
    }
}
