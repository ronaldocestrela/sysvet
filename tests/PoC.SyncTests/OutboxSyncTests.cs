using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace PoC.SyncTests;

public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedOn { get; set; }
}

public class LocalOutboxDbContext : DbContext
{
    public LocalOutboxDbContext(DbContextOptions<LocalOutboxDbContext> options) : base(options) { }

    public DbSet<Tutor> Tutors { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tutor>(builder =>
        {
            builder.HasKey(t => t.Id);
            builder.OwnsOne(t => t.Cpf, cpf => cpf.Property(c => c.Number).HasColumnName("Cpf"));
            builder.OwnsOne(t => t.Email, email => email.Property(e => e.Address).HasColumnName("Email"));
            builder.OwnsOne(t => t.Phone, phone => phone.Property(p => p.Number).HasColumnName("Phone"));
            builder.Metadata.FindNavigation(nameof(Tutor.Pets))?.SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<OutboxMessage>(builder =>
        {
            builder.HasKey(o => o.Id);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<Tutor>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();

        foreach (var entry in entries)
        {
            var eventType = entry.State == EntityState.Added ? "TutorCreated" : "TutorUpdated";
            var outboxMessage = new OutboxMessage
            {
                Type = eventType,
                Data = JsonSerializer.Serialize(entry.Entity)
            };
            OutboxMessages.Add(outboxMessage);
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}

public class CentralDbContext : DbContext
{
    public CentralDbContext(DbContextOptions<CentralDbContext> options) : base(options) { }
    
    public DbSet<Tutor> Tutors { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tutor>(builder =>
        {
            builder.HasKey(t => t.Id);
            builder.OwnsOne(t => t.Cpf, cpf => cpf.Property(c => c.Number).HasColumnName("Cpf"));
            builder.OwnsOne(t => t.Email, email => email.Property(e => e.Address).HasColumnName("Email"));
            builder.OwnsOne(t => t.Phone, phone => phone.Property(p => p.Number).HasColumnName("Phone"));
            builder.Metadata.FindNavigation(nameof(Tutor.Pets))?.SetPropertyAccessMode(PropertyAccessMode.Field);
        });
    }
}

public class OutboxSyncTests
{
    [Fact]
    public async Task Worker_Should_Process_OutboxMessage_And_Sync_To_CentralDb()
    {
        // 1. Arrange - Setup DbContexts
        var localOptions = new DbContextOptionsBuilder<LocalOutboxDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        var centralOptions = new DbContextOptionsBuilder<CentralDbContext>()
            .UseInMemoryDatabase("CentralDb_OutboxTest")
            .Options;

        using var localDb = new LocalOutboxDbContext(localOptions);
        await localDb.Database.OpenConnectionAsync();
        await localDb.Database.EnsureCreatedAsync();

        using var centralDb = new CentralDbContext(centralOptions);
        await centralDb.Database.EnsureCreatedAsync();

        // 2. Insert data in Local DB (Will automatically generate Outbox message)
        var tutor = Tutor.Create("Local Tutor", Email.Create("local@test.com").Value, Cpf.Create("12345678909").Value, Phone.Create("11999999999").Value).Value;
        localDb.Tutors.Add(tutor);
        await localDb.SaveChangesAsync(); // <-- OutboxMessage is saved here

        // Assert outbox has 1 message
        var messages = await localDb.OutboxMessages.Where(m => m.ProcessedOn == null).ToListAsync();
        messages.Should().HaveCount(1);
        messages.First().Type.Should().Be("TutorCreated");

        // 3. Act - Simulate Background Worker Syncing
        var pendingMessages = await localDb.OutboxMessages.Where(m => m.ProcessedOn == null).ToListAsync();
        foreach (var message in pendingMessages)
        {
            if (message.Type == "TutorCreated")
            {
                using var doc = JsonDocument.Parse(message.Data);
                var root = doc.RootElement;
                
                var name = root.GetProperty("Name").GetString();
                var emailVal = root.GetProperty("Email").GetProperty("Address").GetString();
                var cpfVal = root.GetProperty("Cpf").GetProperty("Number").GetString();
                var phoneVal = root.GetProperty("Phone").GetProperty("Number").GetString();
                var id = Guid.Parse(root.GetProperty("Id").GetString()!);

                var syncedTutor = Tutor.Create(name!, Email.Create(emailVal!).Value, Cpf.Create(cpfVal!).Value, Phone.Create(phoneVal!).Value, id).Value;

                // Update tracked entity state in CentralDb (or use API in real scenario)
                centralDb.Tutors.Add(syncedTutor);
            }

            message.ProcessedOn = DateTime.UtcNow;
        }

        await centralDb.SaveChangesAsync();
        await localDb.SaveChangesAsync();

        // 4. Assert
        var centralTutors = await centralDb.Tutors.ToListAsync();
        centralTutors.Should().HaveCount(1);
        centralTutors.First().Name.Should().Be("Local Tutor");

        var unprocessedCount = await localDb.OutboxMessages.CountAsync(m => m.ProcessedOn == null);
        unprocessedCount.Should().Be(0, "because the worker processed it");
    }
}
