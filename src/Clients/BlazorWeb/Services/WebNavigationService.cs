using SharedUI.Services;
using Microsoft.AspNetCore.Components;

namespace BlazorWeb.Services;

/// <summary>
/// WASM host adapter for <see cref="INavigationService"/>.
/// </summary>
public class WebNavigationService : INavigationService
{
    private readonly NavigationManager _navigationManager;

    /// <summary>
    /// Creates the navigation adapter.
    /// </summary>
    /// <param name="navigationManager">Blazor navigation manager.</param>
    public WebNavigationService(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    /// <inheritdoc />
    public void NavigateTo(string uri, bool forceLoad = false)
    {
        _navigationManager.NavigateTo(uri, forceLoad);
    }
}
