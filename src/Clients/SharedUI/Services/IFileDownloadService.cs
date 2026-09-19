namespace SharedUI.Services;

/// <summary>
/// Triggers a file download in the host browser or WebView.
/// </summary>
public interface IFileDownloadService
{
    /// <summary>Downloads bytes with the given file name and MIME type.</summary>
    Task DownloadAsync(string fileName, string contentType, byte[] content, CancellationToken cancellationToken = default);
}
