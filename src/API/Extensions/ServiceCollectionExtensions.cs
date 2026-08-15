using Microsoft.EntityFrameworkCore;

namespace API.Extensions;

/// <summary>
/// Extensões para encapsular as configurações de Injeção de Dependência da API.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configura a documentação OpenAPI nativa do .NET 10.
    /// </summary>
    /// <param name="services">A coleção de serviços.</param>
    /// <returns>A própria coleção de serviços configurada.</returns>
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddOpenApi();
        return services;
    }

    /// <summary>
    /// Registra os serviços, handlers e infraestrutura do módulo Core.
    /// </summary>
    public static IServiceCollection AddCoreModule(this IServiceCollection services)
    {
        services.AddDbContext<Core.Infrastructure.Persistence.CoreDbContext>(options =>
        {
            options.UseSqlite("Data Source=sysvet.db");
        });

        // Register default TenantContext for migrations/startup
        services.AddScoped<Core.Domain.ITenantContext, API.Services.DefaultTenantContext>();

        return services;
    }
}
