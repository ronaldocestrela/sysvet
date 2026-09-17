using Clients.Infrastructure.Sync;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clients.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF mapping for the transactional outbox on the client (ADR-002).
/// </summary>
public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Type).IsRequired().HasMaxLength(200);
        builder.Property(o => o.Payload).IsRequired();
        builder.HasIndex(o => o.CreatedAt);
    }
}
