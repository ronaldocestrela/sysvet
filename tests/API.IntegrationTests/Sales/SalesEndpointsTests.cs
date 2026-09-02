using System;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Core.Infrastructure.Identity;
using System.Net.Http.Headers;
using Sales.Application.CashRegisters.Commands;

namespace API.IntegrationTests.Sales;

[Collection("IntegrationTests")]
public class SalesEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SalesEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private async Task<System.Net.Http.HttpClient> CreateAuthenticatedClientAsync()
    {
        using var scope = _factory.Services.CreateScope();
        
        var coreContext = scope.ServiceProvider.GetRequiredService<Core.Infrastructure.Persistence.CoreDbContext>();
        await coreContext.Database.EnsureDeletedAsync();
        await coreContext.Database.EnsureCreatedAsync();
        
        var salesContext = scope.ServiceProvider.GetRequiredService<global::Sales.Infrastructure.Persistence.SalesDbContext>();
        await salesContext.Database.MigrateAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));

        var user = await userManager.FindByEmailAsync("test@sysvet.com");
        if (user == null)
        {
            user = new AppUser { UserName = "test@sysvet.com", Email = "test@sysvet.com", TenantId = Guid.NewGuid() };
            await userManager.CreateAsync(user, "Password123!");
            await userManager.AddToRoleAsync(user, "Admin");
        }

        var client = _factory.CreateClient();
        var request = new { Email = "test@sysvet.com", Password = "Password123!" };
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", request);
        
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse!.AccessToken);
        return client;
    }

    [Fact]
    public async Task OpenCashRegister_ShouldReturnSuccess()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync();
        var command = new OpenCashRegisterCommand { OpeningBalance = 100m };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/sales/cash-registers/open", command);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var id = System.Text.Json.JsonSerializer.Deserialize<Guid>(content, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        id.Should().NotBeEmpty();
    }
}

public class LoginResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
}
