using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Domain.Entities;

namespace Platform.Infrastructure.Persistence.Configurations;

/// <summary>EF mapping for impersonation sessions.</summary>
internal sealed class ImpersonationSessionConfiguration : IEntityTypeConfiguration<ImpersonationSession>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ImpersonationSession> builder)
    {
        builder.ToTable("PlatformImpersonationSessions");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.ActorUserId).HasMaxLength(64).IsRequired();
        builder.Property(s => s.ActorEmail).HasMaxLength(256).IsRequired();
        builder.Property(s => s.ClientIp).HasMaxLength(64).IsRequired();
        builder.Property(s => s.RowVersion).IsConcurrencyToken();
        builder.HasIndex(s => s.TargetTenantId);
        builder.HasIndex(s => s.ExpiresAt);
    }
}
