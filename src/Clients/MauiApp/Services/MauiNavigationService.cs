using SharedUI.Services;
using Microsoft.AspNetCore.Components;

namespace MauiApp.Services;

public class MauiNavigationService : INavigationService
{
    private readonly NavigationManager _navigationManager;

    public MauiNavigationService(NavigationManager navigationManager)
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
