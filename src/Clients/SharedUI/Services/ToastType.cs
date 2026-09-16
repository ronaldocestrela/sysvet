namespace SharedUI.Services;

/// <summary>
/// Visual category for transient toast notifications.
/// </summary>
public enum ToastType
{
    /// <summary>Neutral information.</summary>
    Info,

    /// <summary>Successful operation.</summary>
    Success,

    /// <summary>Non-blocking warning.</summary>
    Warning,

    /// <summary>Error or failure.</summary>
    Error
}
