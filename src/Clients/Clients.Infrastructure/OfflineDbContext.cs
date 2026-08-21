using Microsoft.EntityFrameworkCore;
using Core.Domain.Entities;
using Core.Domain;
using Clients.Infrastructure.Sync;
using System.Text.Json;

namespace Clients.Infrastructure;

public class OfflineDbContext : DbContext
{
    public DbSet<Tutor> Tutors => Set<Tutor>();
    public DbSet<Pet> Pets => Set<Pet>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public OfflineDbContext(DbContextOptions<OfflineDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Map ValueObjects properly
        modelBuilder.Entity<Tutor>(builder =>
        {
            builder.HasKey(t => t.Id);
            builder.OwnsOne(t => t.Cpf, cpf =>
            {
                cpf.Property(c => c.Number).HasColumnName("Cpf");
            });
            builder.OwnsOne(t => t.Email, email =>
            {
                email.Property(e => e.Address).HasColumnName("Email");
            });
            builder.OwnsOne(t => t.Phone, phone =>
            {
                phone.Property(p => p.Number).HasColumnName("Phone");
            });
            builder.Metadata.FindNavigation(nameof(Tutor.Pets))?.SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<Pet>(builder =>
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
            builder.Property(p => p.Species).IsRequired().HasMaxLength(50);
            builder.Property(p => p.Breed).IsRequired().HasMaxLength(50);
            builder.Property(p => p.Sex).IsRequired().HasConversion<string>();
            builder.HasOne<Tutor>().WithMany(t => t.Pets).HasForeignKey(p => p.TutorId);
        });

        modelBuilder.Entity<OutboxMessage>(builder =>
        {
            builder.HasKey(o => o.Id);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<Entity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();

        var outboxMessages = new List<OutboxMessage>();

        foreach (var entry in entries)
        {
            entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;

            if (entry.Entity is Tutor tutor)
            {
                if (entry.State == EntityState.Added)
                {
                    var cmd = new 
                    { 
                        Id = tutor.Id, 
                        Name = tutor.Name, 
                        Email = tutor.Email.Address, 
                        Cpf = tutor.Cpf.Number, 
                        Phone = tutor.Phone.Number 
                    };
                    outboxMessages.Add(new OutboxMessage
                    {
                        Type = "RegisterTutorCommand",
                        Payload = JsonSerializer.Serialize(cmd)
                    });
                }
                else if (entry.State == EntityState.Modified)
                {
                    var cmd = new 
                    { 
                        Id = tutor.Id, 
                        Name = tutor.Name, 
                        Email = tutor.Email.Address, 
                        Cpf = tutor.Cpf.Number, 
                        Phone = tutor.Phone.Number 
                    };
                    outboxMessages.Add(new OutboxMessage
                    {
                        Type = "UpdateTutorCommand",
                        Payload = JsonSerializer.Serialize(cmd)
                    });
                }
            }
            else if (entry.Entity is Pet pet)
            {
                // We'll implement Pet commands later if they don't exist yet, for now just basic structure
                if (entry.State == EntityState.Added)
                {
                    // var cmd = new CreatePetCommand(pet.Id, pet.TutorId, pet.Name, pet.Species, pet.Breed, pet.Sex.ToString());
                    // outboxMessages.Add(new OutboxMessage { Type = "CreatePetCommand", Payload = JsonSerializer.Serialize(cmd) });
                }
                else if (entry.State == EntityState.Modified)
                {
                    // var cmd = new UpdatePetCommand(pet.Id, pet.Name, pet.Species, pet.Breed, pet.Sex.ToString());
                    // outboxMessages.Add(new OutboxMessage { Type = "UpdatePetCommand", Payload = JsonSerializer.Serialize(cmd) });
                }
            }
        }

        if (outboxMessages.Any())
        {
            OutboxMessages.AddRange(outboxMessages);
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}

