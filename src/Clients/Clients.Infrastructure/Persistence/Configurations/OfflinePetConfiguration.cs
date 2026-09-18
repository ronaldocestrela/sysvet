using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clients.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF mapping for pets in the client-local SQLite database.
/// </summary>
public sealed class OfflinePetConfiguration : IEntityTypeConfiguration<Pet>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Pet> builder)
    {
        builder.ToTable("Pets");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.RowVersion).IsConcurrencyToken();
        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(p => p.Name);
        builder.HasIndex(p => p.TutorId);

        builder.Property(p => p.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(p => p.DeletedAt);

        builder.Property(p => p.Species)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(p => p.Breed).HasMaxLength(100);

        builder.Property(p => p.Sex)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.BirthDate);

        builder.HasOne<Tutor>()
            .WithMany(t => t.Pets)
            .HasForeignKey(p => p.TutorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
