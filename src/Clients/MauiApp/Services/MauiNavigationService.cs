using SharedUI.Services;
using Microsoft.AspNetCore.Components;

namespace MauiApp.Services;

/// <summary>
/// MAUI Blazor Hybrid adapter for <see cref="INavigationService"/>.
/// </summary>
public class MauiNavigationService : INavigationService
{
    private readonly NavigationManager _navigationManager;

    /// <summary>
    /// Creates the navigation adapter.
    /// </summary>
    /// <param name="navigationManager">Blazor navigation manager.</param>
    public MauiNavigationService(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    /// <inheritdoc />
    public void NavigateTo(string uri, bool forceLoad = false)
    {
        _navigationManager.NavigateTo(uri, forceLoad);
    }
}
