using API.Extensions;
using Core.Application.Behaviors;
using Core.Domain;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Veterinary.Application.Appointments.Commands;

namespace API.IntegrationTests;

public class ModuleRegistrationTests
{
    private static ServiceProvider BuildProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplicationModules(configuration);
        return services.BuildServiceProvider();
    }

    [Fact]
    public void AddApplicationModules_ResolvesCoreDbContext()
    {
        using var provider = BuildProvider();
        var db = provider.GetService<Core.Infrastructure.Persistence.CoreDbContext>();
        db.Should().NotBeNull();
    }

    [Fact]
    public void AddApplicationModules_ResolvesVeterinaryDbContextAndHandlers()
    {
        using var provider = BuildProvider();
        provider.GetService<global::Veterinary.Infrastructure.Persistence.VeterinaryDbContext>().Should().NotBeNull();
        provider.GetService<IRequestHandler<ScheduleAppointmentCommand, Result<Guid>>>().Should().NotBeNull();
    }

    [Fact]
    public void AddApplicationModules_ResolvesInventoryAndSalesServices()
    {
        using var provider = BuildProvider();
        provider.GetService<global::Inventory.Infrastructure.Persistence.InventoryDbContext>().Should().NotBeNull();
        provider.GetService<global::Sales.Infrastructure.Persistence.SalesDbContext>().Should().NotBeNull();
        provider.GetService<global::Inventory.Domain.Repositories.IProductRepository>().Should().NotBeNull();
        provider.GetService<global::Sales.Domain.Repositories.IOrderRepository>().Should().NotBeNull();
    }

    [Fact]
    public void AddApplicationModules_DoesNotThrowForPetshopAndFiscalStubs()
    {
        var act = () => BuildProvider();
        act.Should().NotThrow();
    }

    [Fact]
    public void AddApplicationModules_RegistersValidationBehaviorForVeterinaryPipeline()
    {
        using var provider = BuildProvider();
        var behaviors = provider.GetServices<IPipelineBehavior<ScheduleAppointmentCommand, Result<Guid>>>();
        behaviors.Select(b => b.GetType()).Should().Contain(typeof(ValidationBehavior<ScheduleAppointmentCommand, Result<Guid>>));
    }
}
