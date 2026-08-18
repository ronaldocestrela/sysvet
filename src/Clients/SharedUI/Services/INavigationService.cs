namespace SharedUI.Services;

public interface INavigationService
{
    void NavigateTo(string uri);
    void NavigateTo(string uri, bool forceLoad);
}
