namespace MauiApp;

/// <summary>
/// Resolves the API base URL for the MAUI host per platform defaults and optional configuration.
/// </summary>
public static class MauiApiConfiguration
{
    /// <summary>
    /// Returns a normalized API base URL with trailing slash.
    /// </summary>
    public static string ResolveApiBaseUrl(IConfiguration? configuration)
    {
        var configured = configuration?["ApiBaseUrl"];
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return Normalize(configured);
        }

#if ANDROID
        return "http://10.0.2.2:5222/";
#elif WINDOWS
        return "https://localhost:7180/";
#else
        return "https://localhost:7180/";
#endif
    }

    private static string Normalize(string url) =>
        url.EndsWith('/') ? url : url + "/";
}
