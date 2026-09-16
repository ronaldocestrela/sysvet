using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace API.Extensions;

/// <summary>
/// Adds API version metadata and Scalar-friendly module tag groups to the OpenAPI document.
/// </summary>
public sealed class OpenApiModuleDocumentTransformer : IOpenApiDocumentTransformer
{
    /// <inheritdoc />
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Info ??= new OpenApiInfo();
        document.Info.Version = "1.0.0";
        document.Info.Title ??= "SysVet API";

        return Task.CompletedTask;
    }
}
