using Microsoft.Extensions.DependencyInjection;
using SharedUI.Services;

namespace SharedUI.DependencyInjection;

/// <summary>
/// DI registration for cross-host SharedUI services.
/// </summary>
public static class SharedUIServiceCollectionExtensions
{
    /// <summary>
    /// Registers design-system services (toast bus, etc.) required by layouts and pages.
    /// </summary>
    /// <param name="services">Host service collection.</param>
    /// <returns>The same collection for chaining.</returns>
    public static IServiceCollection AddSharedUI(this IServiceCollection services)
    {
        services.AddSingleton<IToastService, ToastService>();
        return services;
    }
}
