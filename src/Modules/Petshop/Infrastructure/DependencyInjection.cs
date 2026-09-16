using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Petshop.Application;

namespace Petshop.Infrastructure;

/// <summary>
/// Registers Petshop module services. Stub until business handlers and persistence are implemented.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Reserved composition hook for the Petshop module; currently registers no runtime services.
    /// </summary>
    public static IServiceCollection AddPetshopModule(this IServiceCollection services, IConfiguration configuration)
    {
        _ = configuration;
        _ = typeof(PetshopModuleAssemblyMarker).Assembly;
        return services;
    }
}
