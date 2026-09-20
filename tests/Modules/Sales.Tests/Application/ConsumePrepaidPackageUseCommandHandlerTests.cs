using Core.Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Sales.Application.Prepaid;
using Sales.Domain.Entities;
using Sales.Domain.Enums;
using Sales.Infrastructure.Persistence;

namespace Sales.Tests.Application;

public class ConsumePrepaidPackageUseCommandHandlerTests
{
    [Fact]
    public async Task Handle_DecrementsRemainingUses()
    {
        var options = new DbContextOptionsBuilder<SalesDbContext>()
            .UseSqlite($"Data Source=file:sales-consume-{Guid.NewGuid():N}?mode=memory&cache=shared")
            .Options;

        await using var context = new SalesDbContext(options, new TestTenantContext());
        await context.Database.OpenConnectionAsync();
        await context.Database.MigrateAsync();

        var tutorId = Guid.NewGuid();
        var petId = Guid.NewGuid();
        var balance = PrepaidBalance.Create(tutorId, petId, ServiceCode.Banho).Value;
        balance.Credit(Guid.NewGuid(), Guid.NewGuid(), 4);
        context.PrepaidBalances.Add(balance);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repo = new Sales.Infrastructure.Persistence.Repositories.PrepaidBalanceRepository(context);
        var handler = new ConsumePrepaidPackageUseCommandHandler(repo);

        var result = await handler.Handle(new ConsumePrepaidPackageUseCommand
        {
            UsageId = Guid.NewGuid(),
            PetId = petId,
            ServiceCode = ServiceCode.Banho
        }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.IsFailure ? result.Error.Code : "ok");
        var save = async () => await context.SaveChangesAsync();
        await save.Should().NotThrowAsync();
        var saved = await context.PrepaidBalances.FirstAsync(b => b.Id == balance.Id);
        saved.RemainingUses.Should().Be(3);
    }

    private sealed class TestTenantContext : ITenantContext
    {
        public Guid TenantId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; } = Guid.NewGuid();
        public string SchemaName { get; set; } = "dbo";
    }
}
