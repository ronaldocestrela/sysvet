using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Infrastructure.Configuration;

/// <summary>
/// Registers strongly typed configuration with data annotation validation at startup.
/// </summary>
public static class OptionsServiceCollectionExtensions
{
    /// <summary>
    /// Binds <typeparamref name="TOptions"/> from a configuration section and validates on host start.
    /// </summary>
    /// <typeparam name="TOptions">Options type with data annotations.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="sectionName">Section name to bind; defaults to <typeparamref name="TOptions"/> type name without "Options" suffix when null.</param>
    public static IServiceCollection AddValidatedOptions<TOptions>(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName)
        where TOptions : class
    {
        services.AddOptions<TOptions>()
            .Bind(configuration.GetSection(sectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
