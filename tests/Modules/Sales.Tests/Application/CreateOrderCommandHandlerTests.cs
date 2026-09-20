using Core.Application.Common.Interfaces;
using Core.Domain;
using Core.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Sales.Application.Orders.Commands;
using Sales.Domain.Entities;
using Sales.Domain.Enums;
using Sales.Domain.Repositories;
using Sales.Infrastructure.Persistence;

namespace Sales.Tests.Application;

public class CreateOrderCommandHandlerTests
{
    [Fact]
    public async Task Handle_PersistsOrderWithItems()
    {
        var options = new DbContextOptionsBuilder<SalesDbContext>()
            .UseSqlite($"Data Source=file:sales-test-{Guid.NewGuid():N}?mode=memory&cache=shared")
            .Options;

        await using var context = new SalesDbContext(options, new TestTenantContext());
        await context.Database.OpenConnectionAsync();
        await context.Database.MigrateAsync();

        var register = CashRegister.Open(Guid.NewGuid(), 100m).Value;
        context.CashRegisters.Add(register);
        await context.SaveChangesAsync();

        var orderRepo = new Sales.Infrastructure.Persistence.Repositories.OrderRepository(context);
        var cashRepo = new Sales.Infrastructure.Persistence.Repositories.CashRegisterRepository(context);
        var tutorRepo = Substitute.For<ITutorRepository>();
        var petRepo = Substitute.For<IPetRepository>();

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.IsAuthenticated.Returns(true);
        currentUser.UserId.Returns(Guid.NewGuid().ToString());
        currentUser.AccessProfileId.Returns(Guid.NewGuid());
        var accessProfiles = Substitute.For<IAccessProfileRepository>();
        accessProfiles.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(AccessProfile.CreateSystem("Admin", "Admin", Core.Domain.Authorization.Permissions.AdminDefaults(), 100m).Value);

        var kitRepo = Substitute.For<IProductKitRepository>();
        var packageRepo = Substitute.For<IServicePackageRepository>();
        var handler = new CreateOrderCommandHandler(
            orderRepo, cashRepo, tutorRepo, petRepo, accessProfiles, kitRepo, packageRepo, currentUser);
        var result = await handler.Handle(new CreateOrderCommand
        {
            CashRegisterId = register.Id,
            Items =
            [
                new CreateOrderItemDto
                {
                    Kind = OrderItemKind.Product,
                    ProductId = Guid.NewGuid(),
                    ProductName = "Test",
                    Quantity = 1,
                    UnitPrice = 10m
                }
            ]
        }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await context.SaveChangesAsync();

        var saved = await context.Orders.Include(o => o.Items).FirstAsync(o => o.Id == result.Value);
        saved.Items.Should().HaveCount(1);
        saved.Items.First().Kind.Should().Be(OrderItemKind.Product);
    }

    private sealed class TestTenantContext : ITenantContext
    {
        public Guid TenantId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; } = Guid.NewGuid();
        public string SchemaName { get; set; } = "dbo";
    }
}
