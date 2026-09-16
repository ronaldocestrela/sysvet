namespace SharedUI.Services;

/// <summary>
/// Application-wide toast notification bus; hosts register a singleton via <see cref="DependencyInjection.SharedUIServiceCollectionExtensions.AddSharedUI"/>.
/// </summary>
public interface IToastService
{
    /// <summary>Currently visible toasts.</summary>
    IReadOnlyList<ToastMessage> Toasts { get; }

    /// <summary>Raised when the toast list changes.</summary>
    event Action? Changed;

    /// <summary>Enqueues a toast for display.</summary>
    /// <param name="message">User-visible text.</param>
    /// <param name="type">Semantic styling.</param>
    void Show(string message, ToastType type = ToastType.Info);

    /// <summary>Removes a toast by id.</summary>
    /// <param name="id">Toast identifier.</param>
    void Dismiss(Guid id);
}
