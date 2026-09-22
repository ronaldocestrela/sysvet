using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutorPortal.Domain.Entities;

namespace TutorPortal.Infrastructure.Persistence.Configurations;

internal sealed class TutorPushSubscriptionConfiguration : IEntityTypeConfiguration<TutorPushSubscription>
{
    public void Configure(EntityTypeBuilder<TutorPushSubscription> builder)
    {
        builder.ToTable("TutorPushSubscriptions");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.UserId).HasMaxLength(450).IsRequired();
        builder.Property(s => s.Endpoint).HasMaxLength(2000).IsRequired();
        builder.Property(s => s.P256dh).HasMaxLength(512).IsRequired();
        builder.Property(s => s.Auth).HasMaxLength(256).IsRequired();
        builder.Property(s => s.UserAgent).HasMaxLength(512);
        builder.HasIndex(s => new { s.UserId, s.Endpoint }).IsUnique();
    }
}
