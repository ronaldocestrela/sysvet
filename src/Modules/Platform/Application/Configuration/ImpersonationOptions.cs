namespace Platform.Application.Configuration;

/// <summary>Impersonation settings (Platform:Impersonation).</summary>
public sealed class ImpersonationOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Platform:Impersonation";

    /// <summary>JWT lifetime in minutes.</summary>
    public int SessionMinutes { get; set; } = 15;
}
