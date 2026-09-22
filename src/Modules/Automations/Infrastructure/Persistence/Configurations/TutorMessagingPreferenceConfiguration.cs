using Automations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automations.Infrastructure.Persistence.Configurations;

internal sealed class TutorMessagingPreferenceConfiguration : IEntityTypeConfiguration<TutorMessagingPreference>
{
    public void Configure(EntityTypeBuilder<TutorMessagingPreference> builder)
    {
        builder.ToTable("TutorMessagingPreferences");
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.TutorId).IsUnique();
        builder.Property(p => p.TutorId).IsRequired();
        builder.Property(p => p.WhatsAppEnabled).IsRequired();
        builder.Property(p => p.EmailEnabled).IsRequired();
    }
}
