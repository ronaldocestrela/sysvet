namespace SharedUI.Services;

public interface IConnectivityService
{
    bool IsOnline { get; }
    event EventHandler<bool> ConnectivityChanged;
}
