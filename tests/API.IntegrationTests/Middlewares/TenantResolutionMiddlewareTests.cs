using System.Security.Claims;
using API.Middlewares;
using Core.Domain;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Platform.Application.Tenancy;
using Xunit;

namespace API.IntegrationTests.Middlewares;

public class TenantResolutionMiddlewareTests
{
    private sealed class FakeTenantContext : ITenantContext
    {
        public Guid TenantId { get; set; }
        public Guid UserId { get; set; }
        public string SchemaName { get; set; } = string.Empty;
    }

    [Fact]
    public async Task InvokeAsync_WithTenantIdClaim_SetsTenantContext()
    {
        var tenantId = Guid.NewGuid();
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("TenantId", tenantId.ToString())
        ], "TestAuthType"));

        var tenantContext = new FakeTenantContext();
        var slugLookup = Substitute.For<ITenantSlugLookup>();
        var services = new ServiceCollection();
        services.AddSingleton<ITenantContext>(tenantContext);
        services.AddSingleton(slugLookup);
        services.AddOptions<Core.Infrastructure.Tenancy.TenancySettings>();
        context.RequestServices = services.BuildServiceProvider();

        var middleware = new TenantResolutionMiddleware(_ => Task.CompletedTask);
        await middleware.InvokeAsync(context);

        tenantContext.TenantId.Should().Be(tenantId);
        tenantContext.SchemaName.Should().Be(TenantSchema.FromId(tenantId));
    }

    [Fact]
    public async Task InvokeAsync_WithHeaderTenantId_SetsTenantContext()
    {
        var tenantId = Guid.NewGuid();
        var context = new DefaultHttpContext();
        context.Request.Headers[TenantResolutionHeaders.TenantId] = tenantId.ToString();

        var tenantContext = new FakeTenantContext();
        var slugLookup = Substitute.For<ITenantSlugLookup>();
        var services = new ServiceCollection();
        services.AddSingleton<ITenantContext>(tenantContext);
        services.AddSingleton(slugLookup);
        services.AddOptions<Core.Infrastructure.Tenancy.TenancySettings>();
        context.RequestServices = services.BuildServiceProvider();

        var middleware = new TenantResolutionMiddleware(_ => Task.CompletedTask);
        await middleware.InvokeAsync(context);

        tenantContext.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task InvokeAsync_JwtAndConflictingHeader_Returns403()
    {
        var jwtTenant = Guid.NewGuid();
        var otherTenant = Guid.NewGuid();
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("TenantId", jwtTenant.ToString())
        ], "TestAuthType"));
        context.Request.Headers[TenantResolutionHeaders.TenantId] = otherTenant.ToString();

        var tenantContext = new FakeTenantContext();
        var slugLookup = Substitute.For<ITenantSlugLookup>();
        var services = new ServiceCollection();
        services.AddSingleton<ITenantContext>(tenantContext);
        services.AddSingleton(slugLookup);
        services.AddOptions<Core.Infrastructure.Tenancy.TenancySettings>();
        context.RequestServices = services.BuildServiceProvider();

        var nextCalled = false;
        var middleware = new TenantResolutionMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        nextCalled.Should().BeFalse();
    }

    [Theory]
    [InlineData("clinica.vetnexus.app", "clinica")]
    [InlineData("localhost", null)]
    [InlineData("api.vetnexus.app", null)]
    public void ResolveHostLabel_ParsesSubdomain(string host, string? expected)
    {
        TenantResolutionMiddleware.ResolveHostLabel(host).Should().Be(expected);
    }
}
