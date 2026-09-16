namespace SharedUI.Services;

/// <summary>
/// In-memory toast store shared across Blazor WebAssembly and MAUI hosts.
/// </summary>
public sealed class ToastService : IToastService
{
    private readonly List<ToastMessage> _toasts = [];

    /// <inheritdoc />
    public IReadOnlyList<ToastMessage> Toasts => _toasts;

    /// <inheritdoc />
    public event Action? Changed;

    /// <inheritdoc />
    public void Show(string message, ToastType type = ToastType.Info)
    {
        _toasts.Add(new ToastMessage { Message = message, Type = type });
        Changed?.Invoke();
    }

    /// <inheritdoc />
    public void Dismiss(Guid id)
    {
        var removed = _toasts.RemoveAll(t => t.Id == id);
        if (removed > 0)
        {
            Changed?.Invoke();
        }
    }
}
