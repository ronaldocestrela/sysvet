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
}
