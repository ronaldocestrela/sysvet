using API.Extensions;
using Core.Application.Behaviors;
using Core.Domain;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Veterinary.Application.Appointments.Commands;
using Veterinary.Application.Clinical.Commands;
using Veterinary.Application.Clinical.Dtos;
using Veterinary.Application.Vaccines.Commands;
using Veterinary.Application.Vaccines.Dtos;

namespace API.IntegrationTests;

public class ModuleRegistrationTests
{
    internal static ServiceProvider BuildProvider(Action<IServiceCollection>? configure = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Data Source=:memory:",
                ["Database:Provider"] = "Sqlite",
                ["Database:ConnectionStringName"] = "DefaultConnection",
                ["TenancySettings:DefaultSchema"] = "dbo",
                ["JwtSettings:Secret"] = "module-registration-secret-16",
                ["JwtSettings:Issuer"] = "sysvet-api",
                ["JwtSettings:Audience"] = "sysvet-clients",
                ["JwtSettings:ExpiryMinutes"] = "60",
                ["Fiscal:Provider"] = "Fake",
                ["Fiscal:CertificateEncryptionKey"] = "dev-fiscal-cert-key-min-32-chars!!"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddApplicationModules(configuration);
        configure?.Invoke(services);
        return services.BuildServiceProvider();
    }

    [Fact]
    public void AddApplicationModules_ResolvesCoreDbContext()
    {
        using var root = BuildProvider();
        using var scope = root.CreateScope();
        var db = scope.ServiceProvider.GetService<Core.Infrastructure.Persistence.CoreDbContext>();
        db.Should().NotBeNull();
    }

    [Fact]
    public void AddApplicationModules_ResolvesVeterinaryDbContextAndHandlers()
    {
        using var root = BuildProvider();
        using var scope = root.CreateScope();
        var provider = scope.ServiceProvider;
        provider.GetService<global::Veterinary.Infrastructure.Persistence.VeterinaryDbContext>().Should().NotBeNull();
        provider.GetService<IRequestHandler<ScheduleAppointmentCommand, Result<Guid>>>().Should().NotBeNull();
        provider.GetService<IRequestHandler<ListClinicalExamsByAppointmentQuery, Result<IReadOnlyList<ClinicalExamDto>>>>().Should().NotBeNull();
        provider.GetService<IRequestHandler<ListVaccineDosesByPetQuery, Result<IReadOnlyList<VaccineDoseDto>>>>().Should().NotBeNull();
    }

    [Fact]
    public void AddApplicationModules_ResolvesInventoryAndSalesServices()
    {
        using var root = BuildProvider();
        using var scope = root.CreateScope();
        var provider = scope.ServiceProvider;
        provider.GetService<global::Inventory.Infrastructure.Persistence.InventoryDbContext>().Should().NotBeNull();
        provider.GetService<global::Sales.Infrastructure.Persistence.SalesDbContext>().Should().NotBeNull();
        provider.GetService<global::Inventory.Domain.Repositories.IProductRepository>().Should().NotBeNull();
        provider.GetService<IRequestHandler<global::Inventory.Application.Products.Commands.RegisterProductCommand, Result<Guid>>>().Should().NotBeNull();
        provider.GetService<global::Sales.Domain.Repositories.IOrderRepository>().Should().NotBeNull();
    }

    [Fact]
    public void AddApplicationModules_ResolvesFinanceDbContext()
    {
        using var root = BuildProvider();
        using var scope = root.CreateScope();
        var provider = scope.ServiceProvider;
        provider.GetService<global::Finance.Infrastructure.Persistence.FinanceDbContext>().Should().NotBeNull();
        provider.GetService<global::Finance.Domain.Repositories.IFinanceUnitOfWork>().Should().NotBeNull();
    }

    [Fact]
    public void AddApplicationModules_ResolvesFiscalDbContext()
    {
        using var root = BuildProvider();
        using var scope = root.CreateScope();
        var provider = scope.ServiceProvider;
        provider.GetService<global::Fiscal.Infrastructure.Persistence.FiscalDbContext>().Should().NotBeNull();
        provider.GetService<global::Fiscal.Domain.Repositories.IFiscalUnitOfWork>().Should().NotBeNull();
    }

    [Fact]
    public void AddApplicationModules_ResolvesAutomationsDbContext()
    {
        using var root = BuildProvider();
        using var scope = root.CreateScope();
        var provider = scope.ServiceProvider;
        provider.GetService<global::Automations.Infrastructure.Persistence.AutomationsDbContext>().Should().NotBeNull();
        provider.GetService<global::Automations.Domain.Repositories.IAutomationsUnitOfWork>().Should().NotBeNull();
    }

    [Fact]
    public void AddApplicationModules_DoesNotThrowForPetshopStub()
    {
        var act = () => BuildProvider();
        act.Should().NotThrow();
    }

    [Fact]
    public void AddApplicationModules_RegistersValidationBehaviorForVeterinaryPipeline()
    {
        using var root = BuildProvider();
        using var scope = root.CreateScope();
        var behaviors = scope.ServiceProvider.GetServices<IPipelineBehavior<ScheduleAppointmentCommand, Result<Guid>>>();
        behaviors.Select(b => b.GetType()).Should().Contain(typeof(ValidationBehavior<ScheduleAppointmentCommand, Result<Guid>>));
    }
}
