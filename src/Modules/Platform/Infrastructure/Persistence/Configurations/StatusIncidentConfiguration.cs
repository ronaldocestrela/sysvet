using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Domain.Entities;

namespace Platform.Infrastructure.Persistence.Configurations;

/// <summary>EF mapping for operational status incidents.</summary>
internal sealed class StatusIncidentConfiguration : IEntityTypeConfiguration<StatusIncident>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<StatusIncident> builder)
    {
        builder.ToTable("PlatformStatusIncidents");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Title).HasMaxLength(256).IsRequired();
        builder.Property(i => i.Impact).HasConversion<int>().IsRequired();
        builder.Property(i => i.Components).HasMaxLength(512).IsRequired();
        builder.Property(i => i.RowVersion).IsConcurrencyToken();
        builder.HasIndex(i => i.StartedAt);
        builder.HasIndex(i => i.ResolvedAt);
    }
}
