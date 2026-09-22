using Automations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automations.Infrastructure.Persistence.Configurations;

internal sealed class AutomationsSettingsConfiguration : IEntityTypeConfiguration<AutomationsSettings>
{
    public void Configure(EntityTypeBuilder<AutomationsSettings> builder)
    {
        builder.ToTable("AutomationsSettings");
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => s.Key).IsUnique();
        builder.Property(s => s.Key).HasMaxLength(64).IsRequired();
        builder.Property(s => s.TimeZoneId).HasMaxLength(128).IsRequired();
        builder.Property(s => s.BusinessStart).IsRequired();
        builder.Property(s => s.BusinessEnd).IsRequired();
        builder.Property(s => s.BusinessDaysJson).HasMaxLength(256).IsRequired();
    }
}
