using Clients.Infrastructure;
using Clients.Infrastructure.Crm;
using Clients.Infrastructure.Http;
using Clients.Infrastructure.Persistence;
using Clients.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Petshop.Domain.Entities;
using Petshop.Domain.Enums;

namespace Clients.Tests.Crm;

public class OfflineGroomingStoreTests
{
    private static (OfflineDbContext Db, OfflineGroomingStore Store) CreateStore()
    {
        var options = new DbContextOptionsBuilder<OfflineDbContext>()
            .UseSqlite($"Data Source=grooming_store_{Guid.NewGuid()}.db")
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;
        var db = new OfflineDbContext(options, new NoOpSqliteFilePersistence());
        db.Database.MigrateAsync().GetAwaiter().GetResult();
        var petRepo = new OfflinePetRepository(db);
        var store = new OfflineGroomingStore(db, petRepo);
        return (db, store);
    }

    [Fact]
    public async Task StartAsync_EnqueuesStartGroomingAppointmentCommand()
    {
        var (ctx, store) = CreateStore();
        await using var _ = ctx;

        var appointmentId = Guid.NewGuid();
        var appointment = GroomingAppointment.RestoreFromSync(
            appointmentId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddHours(1),
            30,
            "",
            GroomingAppointmentStatus.Confirmed,
            DateTimeOffset.UtcNow);

        ctx.SuppressOutbox = true;
        ctx.GroomingAppointments.Add(appointment);
        await ctx.SaveChangesAsync();
        ctx.SuppressOutbox = false;

        var result = await store.StartAsync(appointmentId);
        result.IsSuccess.Should().BeTrue();

        var message = await ctx.OutboxMessages.SingleAsync(m => m.Type == "StartGroomingAppointmentCommand");
        message.Payload.Should().Contain(appointmentId.ToString());
    }

    [Fact]
    public async Task CompleteAsync_DebitsStockAndEnqueuesCompleteCommand()
    {
        var (ctx, store) = CreateStore();
        await using var _ = ctx;

        var productId = Guid.NewGuid();
        var product = Product.Create("Shampoo", "d", "SH-1", "7890000000100", "UN", 0, ProductCategory.Food, "23091000", null, 0, null, id: productId);
        product.IsSuccess.Should().BeTrue();
        ctx.Products.Add(product.Value);
        ctx.ProductBalances.Add(new ProductBalance(productId, 5m));

        var appointmentId = Guid.NewGuid();
        var groomerId = Guid.NewGuid();
        var tutorId = Guid.NewGuid();
        var petId = Guid.NewGuid();
        var appointment = GroomingAppointment.RestoreFromSync(
            appointmentId,
            tutorId,
            petId,
            groomerId,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddHours(1),
            30,
            "",
            GroomingAppointmentStatus.InProgress,
            DateTimeOffset.UtcNow);

        var record = GroomingRecord.Create(Guid.NewGuid(), appointmentId, groomerId, tutorId, petId).Value;
        record.SetSupplyLines([(productId, 2m)]);

        ctx.SuppressOutbox = true;
        ctx.GroomingAppointments.Add(appointment);
        ctx.GroomingRecords.Add(record);
        await ctx.SaveChangesAsync();
        ctx.SuppressOutbox = false;

        var result = await store.CompleteAsync(appointmentId);
        result.IsSuccess.Should().BeTrue();

        var balance = await ctx.ProductBalances.SingleAsync(b => b.ProductId == productId);
        balance.TotalQuantity.Should().Be(3m);

        await ctx.OutboxMessages.SingleAsync(m => m.Type == "CompleteGroomingAppointmentCommand");
    }
}
