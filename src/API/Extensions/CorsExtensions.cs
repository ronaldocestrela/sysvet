namespace API.Extensions;

/// <summary>
/// Cross-origin configuration for Blazor WebAssembly and other browser clients.
/// </summary>
public static class CorsExtensions
{
    /// <summary>Policy name for SPA clients (Blazor WASM).</summary>
    public const string BlazorWebPolicy = "BlazorWeb";

    /// <summary>
    /// Registers CORS from configuration section <c>Cors:AllowedOrigins</c>.
    /// </summary>
    public static IServiceCollection AddBlazorWebCors(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy(BlazorWebPolicy, policy =>
            {
                if (origins.Length > 0)
                {
                    policy.SetIsOriginAllowed(origin =>
                        {
                            if (origins.Contains(origin, StringComparer.OrdinalIgnoreCase))
                            {
                                return true;
                            }

                            if (Uri.TryCreate(origin, UriKind.Absolute, out var uri)
                                && uri.Host.EndsWith(".vetnexus.app", StringComparison.OrdinalIgnoreCase))
                            {
                                return uri.Scheme is "https" or "http";
                            }

                            return false;
                        })
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                }
            });
        });

        return services;
    }

    /// <summary>
    /// Enables the Blazor WASM CORS policy early in the pipeline (before authentication).
    /// </summary>
    public static IApplicationBuilder UseBlazorWebCors(this IApplicationBuilder app) =>
        app.UseCors(BlazorWebPolicy);
}
