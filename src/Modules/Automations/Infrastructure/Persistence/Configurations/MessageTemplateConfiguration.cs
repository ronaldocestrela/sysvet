using Automations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automations.Infrastructure.Persistence.Configurations;

internal sealed class MessageTemplateConfiguration : IEntityTypeConfiguration<MessageTemplate>
{
    public void Configure(EntityTypeBuilder<MessageTemplate> builder)
    {
        builder.ToTable("MessageTemplates");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.RowVersion).HasColumnType("BLOB");
        builder.Property(t => t.Code).HasMaxLength(100).IsRequired();
        builder.Property(t => t.Channel).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(t => t.Subject).HasMaxLength(200);
        builder.Property(t => t.Body).HasMaxLength(4000).IsRequired();
        builder.HasIndex(t => new { t.Code, t.Channel }).IsUnique();
    }
}
