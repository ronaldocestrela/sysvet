namespace Core.Application.Operations;

/// <summary>
/// Thread-safe sliding window counter that raises at most one alert per threshold crossing until the count drops below the threshold again.
/// </summary>
public sealed class OperationalAlertWindow
{
    private readonly TimeSpan _window;
    private readonly int _threshold;
    private readonly object _gate = new();
    private readonly Queue<DateTimeOffset> _timestamps = new();
    private bool _alertActive;

    /// <summary>Creates a window with the given duration and alert threshold.</summary>
    public OperationalAlertWindow(TimeSpan window, int threshold)
    {
        if (threshold < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(threshold), "Threshold must be at least 1.");
        }

        _window = window;
        _threshold = threshold;
    }

    /// <summary>Current event count inside the active window.</summary>
    public int CurrentCount
    {
        get
        {
            lock (_gate)
            {
                Prune(DateTimeOffset.UtcNow);
                return _timestamps.Count;
            }
        }
    }

    /// <summary>Configured alert threshold.</summary>
    public int Threshold => _threshold;

    /// <summary>Records an event and returns true when an alert should be emitted.</summary>
    public bool Record(DateTimeOffset utcNow)
    {
        lock (_gate)
        {
            Prune(utcNow);
            _timestamps.Enqueue(utcNow);

            if (_timestamps.Count >= _threshold && !_alertActive)
            {
                _alertActive = true;
                return true;
            }

            return false;
        }
    }

    /// <summary>Whether the window is currently above or at the alert threshold.</summary>
    public bool IsAboveThreshold(DateTimeOffset utcNow)
    {
        lock (_gate)
        {
            Prune(utcNow);
            return _timestamps.Count >= _threshold;
        }
    }

    private void Prune(DateTimeOffset utcNow)
    {
        var cutoff = utcNow - _window;
        while (_timestamps.Count > 0 && _timestamps.Peek() < cutoff)
        {
            _timestamps.Dequeue();
        }

        if (_timestamps.Count < _threshold)
        {
            _alertActive = false;
        }
    }
}
