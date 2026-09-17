using Clients.Infrastructure.Sync;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clients.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF mapping for the client sync cursor singleton.
/// </summary>
public sealed class SyncStateConfiguration : IEntityTypeConfiguration<SyncState>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SyncState> builder)
    {
        builder.ToTable("SyncState");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.LastPullAt).IsRequired();
    }
}
