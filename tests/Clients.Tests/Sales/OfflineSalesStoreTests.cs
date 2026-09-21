using Clients.Infrastructure;
using Clients.Infrastructure.Http;
using Clients.Infrastructure.Persistence;
using Clients.Infrastructure.Fiscal;
using Clients.Infrastructure.Sales;
using Clients.Infrastructure.Sync;
using FluentAssertions;
using Sales.Domain.Payments;
using Sales.Domain.Entities;
using Sales.Domain.Enums;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Clients.Tests.Sales;

public class OfflineSalesStoreTests
{
    private static (OfflineDbContext Db, OfflineSalesStore Store) CreateStore()
    {
        var options = new DbContextOptionsBuilder<OfflineDbContext>()
            .UseSqlite($"Data Source=sales_store_{Guid.NewGuid()}.db")
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;
        var db = new OfflineDbContext(options, new NoOpSqliteFilePersistence());
        db.Database.EnsureCreatedAsync().GetAwaiter().GetResult();
        var connectivity = new FakeOfflineConnectivity();
        var store = new OfflineSalesStore(
            db,
            new SyncWakeSignal(),
            connectivity,
            new SimulatedPaymentTerminal(),
            new OfflineFiscalNfceService(db));
        return (db, store);
    }

    [Fact]
    public async Task CreateAndPayOrder_EnqueuesCreateThenPayAndDebitsStock()
    {
        var (ctx, store) = CreateStore();
        await using var _ = ctx;
        var productId = Guid.NewGuid();
        var product = Product.Create("P", "d", "SKU-1", "7890000000001", "UN", 0, ProductCategory.Food, "23091000", null, 0, null, id: productId);
        product.IsSuccess.Should().BeTrue();
        ctx.Products.Add(product.Value);
        ctx.ProductBalances.Add(new ProductBalance(productId, 5m));
        await ctx.SaveChangesAsync();

        var open = await store.OpenCashRegisterAsync(0m);
        open.IsSuccess.Should().BeTrue();

        var sale = await store.CreateAndPayOrderAsync(new CreateSalesOrderClientRequest
        {
            CashRegisterId = open.Value,
            Items =
            [
                new SalesOrderItemClientDto
                {
                    Kind = "Product",
                    ProductId = productId,
                    ProductName = "P",
                    Quantity = 2m,
                    UnitPrice = 10m
                }
            ]
        }, [new PayOrderPaymentClientDto { Method = "Cash", Amount = 20m }]);

        sale.IsSuccess.Should().BeTrue();

        var balance = await ctx.ProductBalances.SingleAsync(b => b.ProductId == productId);
        balance.TotalQuantity.Should().Be(3m);

        var messages = (await ctx.OutboxMessages.ToListAsync()).OrderBy(m => m.CreatedAt).ToList();
        messages.Should().HaveCount(3);
        messages[0].Type.Should().Be("OpenCashRegisterCommand");
        messages[1].Type.Should().Be("CreateOrderCommand");
        messages[2].Type.Should().Be("PayOrderCommand");
    }

    [Fact]
    public async Task CreateAndPayOrder_WhenInsufficientStock_ReturnsFailure()
    {
        var (ctx, store) = CreateStore();
        await using var _ = ctx;
        var productId = Guid.NewGuid();
        var created = Product.Create("P", "d", "SKU-2", "7890000000002", "UN", 0, ProductCategory.Food, "23091000", null, 0, null, id: productId);
        created.IsSuccess.Should().BeTrue();
        ctx.Products.Add(created.Value);
        ctx.ProductBalances.Add(new ProductBalance(productId, 1m));
        await ctx.SaveChangesAsync();

        var open = await store.OpenCashRegisterAsync(0m);
        open.IsSuccess.Should().BeTrue();
        var sale = await store.CreateAndPayOrderAsync(new CreateSalesOrderClientRequest
        {
            CashRegisterId = open.Value,
            Items =
            [
                new SalesOrderItemClientDto
                {
                    Kind = "Product",
                    ProductId = productId,
                    ProductName = "P",
                    Quantity = 5m,
                    UnitPrice = 1m
                }
            ]
        }, [new PayOrderPaymentClientDto { Method = "Cash", Amount = 5m }]);

        sale.IsFailure.Should().BeTrue();
        sale.Error.Code.Should().Be("Order.InsufficientStock");
    }

    [Fact]
    public async Task CreateAndPayOrder_WithPackage_CreditsPrepaidBalance()
    {
        var (ctx, store) = CreateStore();
        await using var _ = ctx;
        var packageId = Guid.NewGuid();
        var package = ServicePackage.Create(packageId, "Banho 4x", ServiceCode.Banho, 4, true);
        package.IsSuccess.Should().BeTrue();
        ctx.ServicePackages.Add(package.Value);
        await ctx.SaveChangesAsync();

        var open = await store.OpenCashRegisterAsync(0m);
        open.IsSuccess.Should().BeTrue();
        var tutorId = Guid.NewGuid();
        var petId = Guid.NewGuid();

        var sale = await store.CreateAndPayOrderAsync(new CreateSalesOrderClientRequest
        {
            CashRegisterId = open.Value,
            TutorId = tutorId,
            PetId = petId,
            Items =
            [
                new SalesOrderItemClientDto
                {
                    Kind = "Package",
                    CatalogOfferId = packageId,
                    ProductName = "Banho 4x",
                    Quantity = 1m,
                    UnitPrice = 100m
                }
            ]
        }, [new PayOrderPaymentClientDto { Method = "Cash", Amount = 100m }]);

        sale.IsSuccess.Should().BeTrue();
        var balance = await ctx.PrepaidBalances.SingleAsync(b => b.PetId == petId);
        balance.RemainingUses.Should().Be(4);
    }

    [Fact]
    public async Task ConsumePrepaidUse_DecrementsBalanceAndEnqueuesOutbox()
    {
        var (ctx, store) = CreateStore();
        await using var _ = ctx;
        var balance = PrepaidBalance.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), ServiceCode.Banho);
        balance.IsSuccess.Should().BeTrue();
        ctx.Entry(balance.Value).Property(b => b.RemainingUses).CurrentValue = 2;
        ctx.PrepaidBalances.Add(balance.Value);
        await ctx.SaveChangesAsync();

        var usageId = Guid.NewGuid();
        var result = await store.ConsumePrepaidUseAsync(new ConsumePrepaidUseClientRequest
        {
            UsageId = usageId,
            PetId = balance.Value.PetId,
            ServiceCode = "Banho"
        });

        result.IsSuccess.Should().BeTrue();
        (await ctx.PrepaidBalances.SingleAsync()).RemainingUses.Should().Be(1);
        var outbox = await ctx.OutboxMessages.SingleAsync(m => m.Type == "ConsumePrepaidPackageUseCommand");
        outbox.Id.Should().Be(usageId);
    }

    private sealed class FakeOfflineConnectivity : ISyncConnectivity
    {
        public bool IsOnline => false;
        public event EventHandler? OnlineStateChanged
        {
            add { }
            remove { }
        }
        public void SetSyncing(bool syncing) { }
    }
}
