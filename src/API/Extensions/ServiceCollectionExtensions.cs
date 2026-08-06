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
}
