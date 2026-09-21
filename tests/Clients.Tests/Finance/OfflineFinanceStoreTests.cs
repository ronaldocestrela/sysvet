using Clients.Infrastructure;
using Clients.Infrastructure.Finance;
using Clients.Infrastructure.Persistence;
using Clients.Infrastructure.Sync;
using Finance.Domain.Entities;
using Finance.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Clients.Tests.Finance;

public class OfflineFinanceStoreTests
{
    [Fact]
    public async Task ListTitlesAsync_ReturnsSyncedTitles()
    {
        var connectionString = $"Data Source=file:offline-fin-{Guid.NewGuid():N}?mode=memory&cache=shared";
        var services = new ServiceCollection();
        services.AddSingleton<ISqliteFilePersistence, NoOpSqliteFilePersistence>();
        services.AddDbContext<OfflineDbContext>(o => o.UseSqlite(connectionString));
        services.AddScoped<IFinanceStore, OfflineFinanceStore>();
        await using var provider = services.BuildServiceProvider();

        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OfflineDbContext>();
        await db.Database.OpenConnectionAsync();
        await db.Database.EnsureCreatedAsync();

        var category = FinancialCategory.CreateSystem("Sales", "Vendas", CategoryDirection.In);
        db.FinancialCategories.Add(category);
        var title = FinancialTitle.CreateManual(
            TitleDirection.Payable,
            category.Id,
            PartyKind.Supplier,
            Guid.NewGuid(),
            100m,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
            "Test AP").Value;
        db.FinancialTitles.Add(title);
        await db.SaveChangesAsync();

        var store = scope.ServiceProvider.GetRequiredService<IFinanceStore>();
        var result = await store.ListTitlesAsync();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetCashFlowAsync_MatchesSaleAndPayableSettlement()
    {
        var connectionString = $"Data Source=file:offline-fin-{Guid.NewGuid():N}?mode=memory&cache=shared";
        var services = new ServiceCollection();
        services.AddSingleton<ISqliteFilePersistence, NoOpSqliteFilePersistence>();
        services.AddDbContext<OfflineDbContext>(o => o.UseSqlite(connectionString));
        services.AddScoped<IFinanceStore, OfflineFinanceStore>();
        await using var provider = services.BuildServiceProvider();

        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OfflineDbContext>();
        await db.Database.OpenConnectionAsync();
        await db.Database.EnsureCreatedAsync();

        var salesCat = FinancialCategory.CreateSystem("SALES", "Vendas", CategoryDirection.In);
        var purchaseCat = FinancialCategory.CreateSystem("PURCHASES", "Compras", CategoryDirection.Out);
        db.FinancialCategories.AddRange(salesCat, purchaseCat);

        var issue = new DateOnly(2026, 9, 10);
        var payDate = new DateTimeOffset(2026, 9, 15, 12, 0, 0, TimeSpan.Zero);
        var sale = FinancialTitle.CreateFromSale(
            Guid.NewGuid(),
            salesCat.Id,
            100m,
            Guid.NewGuid(),
            "Sale",
            [new global::Finance.Domain.Models.SalePaymentSlice("Cash", 100m, null, Guid.NewGuid())],
            issue).Value;
        var payable = FinancialTitle.CreateManual(
            TitleDirection.Payable,
            purchaseCat.Id,
            PartyKind.Supplier,
            Guid.NewGuid(),
            40m,
            issue,
            issue.AddDays(20),
            "AP").Value;
        payable.Allocate(40m, payDate, "Pix", Guid.NewGuid()).IsSuccess.Should().BeTrue();

        db.FinancialTitles.AddRange(sale, payable);
        await db.SaveChangesAsync();

        var store = scope.ServiceProvider.GetRequiredService<IFinanceStore>();
        var from = new DateOnly(2026, 9, 1);
        var to = new DateOnly(2026, 9, 30);
        var cashFlow = await store.GetCashFlowAsync(from, to);
        cashFlow.IsSuccess.Should().BeTrue();
        cashFlow.Value.TotalRealizedInflow.Should().Be(100m);
        cashFlow.Value.TotalRealizedOutflow.Should().Be(40m);

        var dre = await store.GetSimplifiedDreAsync(2026, 9);
        dre.IsSuccess.Should().BeTrue();
        dre.Value.TotalRevenue.Should().Be(100m);
        dre.Value.TotalExpense.Should().Be(40m);
    }
}
