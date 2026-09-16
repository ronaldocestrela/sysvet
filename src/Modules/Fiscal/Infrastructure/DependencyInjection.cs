using Fiscal.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fiscal.Infrastructure;

/// <summary>
/// Registers Fiscal module services. Stub until NF-e/NFC-e/NFS-e infrastructure is implemented.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Reserved composition hook for the Fiscal module; currently registers no runtime services.
    /// </summary>
    public static IServiceCollection AddFiscalModule(this IServiceCollection services, IConfiguration configuration)
    {
        _ = configuration;
        _ = typeof(FiscalModuleAssemblyMarker).Assembly;
        return services;
    }
}
