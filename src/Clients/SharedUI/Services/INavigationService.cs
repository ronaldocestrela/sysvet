namespace SharedUI.Services;

/// <summary>
/// Abstraction over Blazor navigation for testability and MAUI/WASM parity.
/// </summary>
public interface INavigationService
{
    /// <summary>Navigates to the given URI.</summary>
    /// <param name="uri">Relative or absolute URI.</param>
    /// <param name="forceLoad">When true, forces a full reload (host-specific).</param>
    void NavigateTo(string uri, bool forceLoad = false);
}
