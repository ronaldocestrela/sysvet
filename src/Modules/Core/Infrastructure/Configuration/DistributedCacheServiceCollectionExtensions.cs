using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Core.Infrastructure.Configuration;

/// <summary>
/// Registers <see cref="Microsoft.Extensions.Caching.Distributed.IDistributedCache"/> for SysVet.
/// </summary>
public static class DistributedCacheServiceCollectionExtensions
{
    /// <summary>
    /// Adds Memory or Redis distributed cache based on <see cref="CacheOptions"/>.
    /// </summary>
    public static IServiceCollection AddSysvetDistributedCache(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<CacheOptions>()
            .Bind(configuration.GetSection(CacheOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(
                o => !o.UseRedis || !string.IsNullOrWhiteSpace(o.ConnectionString),
                "Cache:ConnectionString is required when Cache:Provider is Redis.")
            .ValidateOnStart();

        var provider = configuration.GetSection(CacheOptions.SectionName).Get<CacheOptions>() ?? new CacheOptions();

        if (provider.UseRedis)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = provider.ConnectionString;
                options.InstanceName = "sysvet:";
            });

            services.AddHealthChecks()
                .AddRedis(provider.ConnectionString!, name: "redis", tags: ["ready"]);
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        return services;
    }
}
