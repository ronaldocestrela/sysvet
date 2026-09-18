using API.Serialization;
using Core.Infrastructure;
using Fiscal.Infrastructure;
using Inventory.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Petshop.Infrastructure;
using Sales.Infrastructure;
using Veterinary.Infrastructure;

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
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new DateOnlyJsonConverter());
            options.SerializerOptions.Converters.Add(new NullableDateOnlyJsonConverter());
            options.SerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
        });

        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<OpenApiBearerSecurityTransformer>();
            options.AddDocumentTransformer<OpenApiModuleDocumentTransformer>();
        });
        return services;
    }

    /// <summary>
    /// Registers all business modules in dependency order (Core first for shared MediatR behaviors).
    /// </summary>
    public static IServiceCollection AddApplicationModules(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCoreModule(configuration);
        services.AddVeterinaryModule(configuration);
        services.AddInventoryModule(configuration);
        services.AddSalesModule(configuration);
        services.AddPetshopModule(configuration);
        services.AddFiscalModule(configuration);
        return services;
    }
}
