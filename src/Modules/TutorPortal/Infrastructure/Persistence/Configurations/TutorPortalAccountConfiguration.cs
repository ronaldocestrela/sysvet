using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutorPortal.Domain.Entities;

namespace TutorPortal.Infrastructure.Persistence.Configurations;

internal sealed class TutorPortalAccountConfiguration : IEntityTypeConfiguration<TutorPortalAccount>
{
    public void Configure(EntityTypeBuilder<TutorPortalAccount> builder)
    {
        builder.ToTable("TutorPortalAccounts");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.UserId).HasMaxLength(450).IsRequired();
        builder.Property(a => a.TutorId).IsRequired();
        builder.HasIndex(a => a.UserId).IsUnique();
        builder.HasIndex(a => a.TutorId).IsUnique();
    }
}
