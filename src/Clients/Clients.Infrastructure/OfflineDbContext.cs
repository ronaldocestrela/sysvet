using Clients.Infrastructure.Persistence;
using Clients.Infrastructure.Persistence.Configurations;
using Clients.Infrastructure.Sync;
using Core.Domain;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Clients.Infrastructure;

/// <summary>
/// Client-local SQLite database for offline CRM and the transactional outbox (ADR-002).
/// </summary>
public class OfflineDbContext : DbContext
{
    private readonly ISqliteFilePersistence _filePersistence;

    /// <summary>
    /// When true, <see cref="SaveChangesAsync"/> skips outbox enqueue (remote pull apply).
    /// </summary>
    public bool SuppressOutbox { get; set; }

    /// <summary>
    /// Creates a context bound to DI options and host-specific file persistence.
    /// </summary>
    public OfflineDbContext(DbContextOptions<OfflineDbContext> options, ISqliteFilePersistence filePersistence)
        : base(options)
    {
        _filePersistence = filePersistence;
    }

    /// <summary>Tutors stored locally for offline CRM.</summary>
    public DbSet<Tutor> Tutors => Set<Tutor>();

    /// <summary>Pets stored locally for offline CRM.</summary>
    public DbSet<Pet> Pets => Set<Pet>();

    /// <summary>Pending sync commands (client-only).</summary>
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    /// <summary>Pull cursor singleton.</summary>
    public DbSet<SyncState> SyncState => Set<SyncState>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new OfflineTutorConfiguration());
        modelBuilder.ApplyConfiguration(new OfflinePetConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new SyncStateConfiguration());

        modelBuilder.Entity<Tutor>().HasQueryFilter(t => !t.IsDeleted);
        modelBuilder.Entity<Pet>().HasQueryFilter(p => !p.IsDeleted);
    }

    /// <inheritdoc />
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var outboxMessages = new List<OutboxMessage>();

        if (!SuppressOutbox)
        {
            var entries = ChangeTracker.Entries<Entity>()
                .Where(e => e.State is EntityState.Added or EntityState.Modified)
                .ToList();

            foreach (var entry in entries)
            {
                entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                EnqueueOutbox(entry, outboxMessages);
            }
        }

        if (outboxMessages.Count > 0)
        {
            OutboxMessages.AddRange(outboxMessages);
        }

        var count = await base.SaveChangesAsync(cancellationToken);

        if (count > 0)
        {
            await SqliteFileHelper.FlushToPersistentStorageAsync(this, _filePersistence, cancellationToken);
        }

        return count;
    }

    private static void EnqueueOutbox(EntityEntry entry, ICollection<OutboxMessage> outboxMessages)
    {
        if (entry.Entity is Tutor tutor)
        {
            EnqueueTutorOutbox(entry, tutor, outboxMessages);
        }
        else if (entry.Entity is Pet pet)
        {
            EnqueuePetOutbox(entry, pet, outboxMessages);
        }
    }

    private static void EnqueueTutorOutbox(EntityEntry entry, Tutor tutor, ICollection<OutboxMessage> outboxMessages)
    {
        var outboxId = Guid.NewGuid();

        if (entry.State == EntityState.Added)
        {
            outboxMessages.Add(new OutboxMessage
            {
                Id = outboxId,
                Type = "CreateTutorCommand",
                Payload = OutboxPayloadFactory.CreateTutor(
                    tutor.Id,
                    tutor.Name,
                    tutor.Email.Address,
                    tutor.Cpf.Number,
                    tutor.Phone.Number,
                    outboxId)
            });
            return;
        }

        if (BecameDeleted(entry))
        {
            outboxMessages.Add(new OutboxMessage
            {
                Id = outboxId,
                Type = "DeleteTutorCommand",
                Payload = OutboxPayloadFactory.DeleteTutor(tutor.Id, outboxId)
            });
            return;
        }

        outboxMessages.Add(new OutboxMessage
        {
            Id = outboxId,
            Type = "UpdateTutorCommand",
            Payload = OutboxPayloadFactory.UpdateTutor(
                tutor.Id,
                tutor.Name,
                tutor.Email.Address,
                tutor.Phone.Number,
                tutor.UpdatedAt,
                outboxId)
        });
    }

    private static void EnqueuePetOutbox(EntityEntry entry, Pet pet, ICollection<OutboxMessage> outboxMessages)
    {
        var outboxId = Guid.NewGuid();

        if (entry.State == EntityState.Added)
        {
            outboxMessages.Add(new OutboxMessage
            {
                Id = outboxId,
                Type = "CreatePetCommand",
                Payload = OutboxPayloadFactory.CreatePet(
                    pet.Id,
                    pet.Name,
                    pet.Species.ToString(),
                    pet.Breed,
                    pet.Sex.ToString(),
                    pet.TutorId,
                    outboxId)
            });
            return;
        }

        if (BecameDeleted(entry))
        {
            outboxMessages.Add(new OutboxMessage
            {
                Id = outboxId,
                Type = "DeletePetCommand",
                Payload = OutboxPayloadFactory.DeletePet(pet.Id, outboxId)
            });
            return;
        }

        outboxMessages.Add(new OutboxMessage
        {
            Id = outboxId,
            Type = "UpdatePetCommand",
            Payload = OutboxPayloadFactory.UpdatePet(
                pet.Id,
                pet.Name,
                pet.Species.ToString(),
                pet.Breed,
                pet.Sex.ToString(),
                pet.UpdatedAt,
                outboxId)
        });
    }

    private static bool BecameDeleted(EntityEntry entry)
    {
        if (entry.State != EntityState.Modified)
        {
            return false;
        }

        var deletedProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == nameof(ISoftDeletable.IsDeleted));
        return deletedProperty is { IsModified: true } && deletedProperty.CurrentValue is true;
    }
}
