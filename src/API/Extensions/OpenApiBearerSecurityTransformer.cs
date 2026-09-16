using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace API.Extensions;

/// <summary>
/// Adds JWT Bearer security scheme to the OpenAPI document for Scalar and other clients.
/// </summary>
internal sealed class OpenApiBearerSecurityTransformer : IOpenApiDocumentTransformer
{
    /// <inheritdoc />
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "JWT access token from POST /api/v1/auth/login"
        };

        return Task.CompletedTask;
    }
}
