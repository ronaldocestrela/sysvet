using Microsoft.AspNetCore.Components;
using SharedUI.Services;

namespace TutorPortalWeb.Services;

/// <summary>
/// WASM navigation adapter for tutor portal routes.
/// </summary>
public sealed class WebNavigationService : INavigationService
{
    private readonly NavigationManager _navigationManager;

    public WebNavigationService(NavigationManager navigationManager) => _navigationManager = navigationManager;

    /// <inheritdoc />
    public void NavigateTo(string uri, bool forceLoad = false) => _navigationManager.NavigateTo(uri, forceLoad);
}
