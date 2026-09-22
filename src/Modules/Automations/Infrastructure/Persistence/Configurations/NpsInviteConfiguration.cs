using Automations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automations.Infrastructure.Persistence.Configurations;

internal sealed class NpsInviteConfiguration : IEntityTypeConfiguration<NpsInvite>
{
    public void Configure(EntityTypeBuilder<NpsInvite> builder)
    {
        builder.ToTable("NpsInvites");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.TokenHash).HasMaxLength(128).IsRequired();
        builder.Property(i => i.SourceType).HasMaxLength(64).IsRequired();
        builder.Property(i => i.Comment).HasMaxLength(2000);
        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.HasIndex(i => new { i.SourceType, i.SourceId }).IsUnique();
        builder.HasIndex(i => i.TutorId);
    }
}
