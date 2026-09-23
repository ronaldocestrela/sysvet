using Intelligence.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Intelligence.Infrastructure.Persistence.Configurations;

internal sealed class ProfileDashboardLayoutConfiguration : IEntityTypeConfiguration<ProfileDashboardLayout>
{
    public void Configure(EntityTypeBuilder<ProfileDashboardLayout> builder)
    {
        builder.ToTable("ProfileDashboardLayouts");
        builder.HasKey(l => l.Id);
        builder.Property<Guid>("TenantId").IsRequired();
        builder.Property(l => l.AccessProfileId).IsRequired();
        builder.Property(l => l.SlotsStorage).HasColumnName("SlotsJson").HasMaxLength(8000).IsRequired();
        builder.Ignore(l => l.Slots);
        builder.HasIndex(l => l.AccessProfileId).IsUnique();
    }
}
