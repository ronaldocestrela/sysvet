using SharedUI.Services;
using Microsoft.AspNetCore.Components;

namespace BlazorWeb.Services;

public class WebNavigationService : INavigationService
{
    private readonly NavigationManager _navigationManager;

    public WebNavigationService(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    public void NavigateTo(string uri)
    {
        _navigationManager.NavigateTo(uri);
    }

    public void NavigateTo(string uri, bool forceLoad)
    {
        _navigationManager.NavigateTo(uri, forceLoad);
    }
}
