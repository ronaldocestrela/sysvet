using Core.Domain;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using Core.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace API.IntegrationTests.Platform;

public class TenantIsolationEndpointsTests : IClassFixture<TenantIsolationWebApplicationFactory>
{
    private readonly TenantIsolationWebApplicationFactory _factory;

    public TenantIsolationEndpointsTests(TenantIsolationWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task TenantIsolation_DoesNotLeakCrm_WhenDifferentTenants()
    {
        await using (var setup = _factory.Services.CreateAsyncScope())
        {
            var core = setup.ServiceProvider.GetRequiredService<CoreDbContext>();
            await core.Database.EnsureDeletedAsync();
            await core.Database.EnsureCreatedAsync();
        }

        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var email = Email.Create("isolated@test.com").Value;
        var cpf = Cpf.Create("52998224725").Value;
        var phone = Phone.Create("11999990001").Value;

        await using (var scopeA = _factory.Services.CreateAsyncScope())
        {
            var tenantContext = scopeA.ServiceProvider.GetRequiredService<ITenantContext>();
            tenantContext.TenantId = tenantA;
            tenantContext.SchemaName = TenantSchema.FromId(tenantA);

            var context = scopeA.ServiceProvider.GetRequiredService<CoreDbContext>();
            var tutor = Tutor.Create("Tenant A Tutor", email, cpf, phone).Value;
            context.Tutors.Add(tutor);
            await context.SaveChangesAsync();
        }

        await using (var scopeB = _factory.Services.CreateAsyncScope())
        {
            var tenantContext = scopeB.ServiceProvider.GetRequiredService<ITenantContext>();
            tenantContext.TenantId = tenantB;
            tenantContext.SchemaName = TenantSchema.FromId(tenantB);

            var context = scopeB.ServiceProvider.GetRequiredService<CoreDbContext>();
            var tutors = await context.Tutors.ToListAsync();
            tutors.Should().BeEmpty();
        }
    }
}
