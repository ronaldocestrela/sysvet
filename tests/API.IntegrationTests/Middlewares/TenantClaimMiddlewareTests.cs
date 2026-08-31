using System.Security.Claims;
using Core.Domain;
using Core.Infrastructure.Identity;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace API.IntegrationTests.Middlewares;

public class TenantClaimMiddlewareTests
{
    private class FakeTenantContext : ITenantContext
    {
        public Guid TenantId { get; set; }
        public Guid UserId { get; set; }
        public string SchemaName { get; set; } = string.Empty;
    }

    [Fact]
    public async Task InvokeAsync_WithTenantIdClaim_SetsTenantContext()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var context = new DefaultHttpContext();
        
        var claims = new List<Claim>
        {
            new Claim("TenantId", tenantId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        context.User = new ClaimsPrincipal(identity);

        var tenantContext = new FakeTenantContext();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<ITenantContext>(tenantContext);
        context.RequestServices = serviceCollection.BuildServiceProvider();
        
        RequestDelegate next = (HttpContext hc) => Task.CompletedTask;
        var middleware = new TenantClaimMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        tenantContext.TenantId.Should().Be(tenantId);
        tenantContext.SchemaName.Should().Be($"tenant_{tenantId.ToString("N").ToLowerInvariant()}");
    }

    [Fact]
    public async Task InvokeAsync_WithoutTenantIdClaim_DoesNotSetTenantContext()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var claims = new List<Claim>(); // No TenantId claim
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        context.User = new ClaimsPrincipal(identity);

        var tenantContext = new FakeTenantContext();
        tenantContext.TenantId = Guid.Empty;
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<ITenantContext>(tenantContext);
        context.RequestServices = serviceCollection.BuildServiceProvider();

        RequestDelegate next = (HttpContext hc) => Task.CompletedTask;
        var middleware = new TenantClaimMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        tenantContext.TenantId.Should().Be(Guid.Empty);
    }
}
