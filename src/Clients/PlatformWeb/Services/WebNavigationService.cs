using Microsoft.AspNetCore.Components;
using SharedUI.Services;

namespace PlatformWeb.Services;

/// <summary>Blazor navigation adapter for PlatformWeb.</summary>
public sealed class WebNavigationService : INavigationService
{
    private readonly NavigationManager _navigationManager;

    /// <summary>Creates the service.</summary>
    public WebNavigationService(NavigationManager navigationManager) => _navigationManager = navigationManager;

    /// <inheritdoc />
    public void NavigateTo(string uri, bool forceLoad = false) => _navigationManager.NavigateTo(uri, forceLoad);
}
