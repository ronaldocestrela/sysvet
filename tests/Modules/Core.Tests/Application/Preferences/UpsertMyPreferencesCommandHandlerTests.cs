using Core.Application.Common.Interfaces;
using Core.Application.Preferences.Commands;
using Core.Application.Preferences.Queries;
using Core.Domain;
using Core.Infrastructure.Persistence;
using Core.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Core.Tests.Application.Preferences;

public class UpsertMyPreferencesCommandHandlerTests
{
    [Fact]
    public async Task UpsertAndGet_ShouldRoundTripPreferences()
    {
        var tenantContext = Substitute.For<ITenantContext>();
        tenantContext.TenantId.Returns(Guid.NewGuid());
        tenantContext.SchemaName.Returns("dbo");

        var services = new ServiceCollection();
        services.AddSingleton(tenantContext);
        services.AddDbContext<CoreDbContext>((sp, options) =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddScoped<IUserPreferenceRepository, UserPreferenceRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CoreDbContext>());
        services.AddScoped<UpsertMyPreferencesCommandHandler>();
        services.AddScoped<GetMyPreferencesQueryHandler>();

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.IsAuthenticated.Returns(true);
        currentUser.UserId.Returns("user-123");
        services.AddSingleton(currentUser);

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var upsert = scope.ServiceProvider.GetRequiredService<UpsertMyPreferencesCommandHandler>();
        var get = scope.ServiceProvider.GetRequiredService<GetMyPreferencesQueryHandler>();

        var upsertResult = await upsert.Handle(
            new UpsertMyPreferencesCommand("{\"a\":1}", "{}", "{\"c\":2}"),
            CancellationToken.None);
        upsertResult.IsSuccess.Should().BeTrue();

        var getResult = await get.Handle(new GetMyPreferencesQuery(), CancellationToken.None);
        getResult.IsSuccess.Should().BeTrue();
        getResult.Value.ShortcutsJson.Should().Contain("\"a\"");
    }
}
