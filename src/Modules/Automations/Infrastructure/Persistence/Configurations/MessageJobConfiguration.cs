using Automations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automations.Infrastructure.Persistence.Configurations;

internal sealed class MessageJobConfiguration : IEntityTypeConfiguration<MessageJob>
{
    public void Configure(EntityTypeBuilder<MessageJob> builder)
    {
        builder.ToTable("MessageJobs");
        builder.HasKey(j => j.Id);
        builder.Property(j => j.RowVersion).HasColumnType("BLOB");
        builder.Property(j => j.TemplateCode).HasMaxLength(100).IsRequired();
        builder.Property(j => j.PayloadJson).HasMaxLength(8000).IsRequired();
        builder.Property(j => j.IdempotencyKey).HasMaxLength(64).IsRequired();
        builder.Property(j => j.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(j => j.Channel).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(j => j.LastError).HasMaxLength(2000);
        builder.Property(j => j.SourceType).HasMaxLength(100);
        builder.HasIndex(j => j.IdempotencyKey)
            .IsUnique()
            .HasFilter($"[{nameof(MessageJob.IdempotencyKey)}] <> ''");

        builder.OwnsMany(j => j.AttemptLogs, logs =>
        {
            logs.ToTable("JobAttemptLogs");
            logs.WithOwner().HasForeignKey("MessageJobId");
            logs.HasKey(l => l.Id);
            logs.Property(l => l.RowVersion).HasColumnType("BLOB");
            logs.Property(l => l.Outcome).HasConversion<string>().HasMaxLength(20).IsRequired();
            logs.Property(l => l.Detail).HasMaxLength(2000);
        });
        builder.Navigation(j => j.AttemptLogs).HasField("_attemptLogs");
    }
}
