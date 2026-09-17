using Clients.Infrastructure.Persistence;
using Clients.Infrastructure.Persistence.Configurations;
using Clients.Infrastructure.Sync;
using Core.Domain;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Clients.Infrastructure;

/// <summary>
/// Client-local SQLite database for offline CRM and the transactional outbox (ADR-002).
/// </summary>
public class OfflineDbContext : DbContext
{
    private readonly ISqliteFilePersistence _filePersistence;

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

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new OfflineTutorConfiguration());
        modelBuilder.ApplyConfiguration(new OfflinePetConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());

        modelBuilder.Entity<Tutor>().HasQueryFilter(t => !t.IsDeleted);
        modelBuilder.Entity<Pet>().HasQueryFilter(p => !p.IsDeleted);
    }

    /// <inheritdoc />
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
                        Type = "CreateTutorCommand",
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
                        Phone = tutor.Phone.Number
                    };
                    outboxMessages.Add(new OutboxMessage
                    {
                        Type = "UpdateTutorCommand",
                        Payload = JsonSerializer.Serialize(cmd)
                    });
                }
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
}
