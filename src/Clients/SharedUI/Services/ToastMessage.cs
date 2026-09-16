namespace SharedUI.Services;

/// <summary>
/// Immutable toast entry rendered by <see cref="Components.Toast"/>.
/// </summary>
public sealed class ToastMessage
{
    /// <summary>Unique identifier for dismiss operations.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>User-visible message.</summary>
    public required string Message { get; init; }

    /// <summary>Semantic type controlling styling.</summary>
    public ToastType Type { get; init; } = ToastType.Info;
}
