using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace API.Extensions;

/// <summary>
/// Extensões para registrar os endpoints da API de forma modular.
/// </summary>
public static class EndpointExtensions
{
    /// <summary>
    /// Mapeia os endpoints do módulo Core.
    /// </summary>
    /// <param name="builder">O construtor de rotas (IEndpointRouteBuilder).</param>
    /// <returns>O construtor de rotas configurado.</returns>
    public static IEndpointRouteBuilder MapCoreEndpoints(this IEndpointRouteBuilder builder)
    {
        // Mapeia endpoints dos recursos Core
        builder.MapTutorEndpoints();
        builder.MapPetEndpoints();
        builder.MapSyncEndpoints();
        builder.MapVeterinaryEndpoints();

        return builder;
    }
}
