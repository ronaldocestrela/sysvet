using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF mapping for <see cref="UserPreference"/>.
/// </summary>
public class UserPreferenceConfiguration : IEntityTypeConfiguration<UserPreference>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<UserPreference> builder)
    {
        builder.ToTable("UserPreferences");

        builder.HasKey(p => p.Id);

        builder.Ignore(p => p.RowVersion);

        builder.Property(p => p.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.HasIndex(p => p.UserId)
            .IsUnique();

        builder.Property(p => p.ShortcutsJson)
            .IsRequired()
            .HasMaxLength(UserPreference.MaxJsonLength);

        builder.Property(p => p.SavedFiltersJson)
            .IsRequired()
            .HasMaxLength(UserPreference.MaxJsonLength);

        builder.Property(p => p.ColumnLayoutsJson)
            .IsRequired()
            .HasMaxLength(UserPreference.MaxJsonLength);
    }
}
