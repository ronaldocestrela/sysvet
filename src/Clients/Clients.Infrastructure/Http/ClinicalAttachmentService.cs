using System.Net.Http.Json;
using Clients.Infrastructure.Crm;
using Clients.Infrastructure.Sync;
using Core.Domain;

namespace Clients.Infrastructure.Http;

/// <summary>Uploads attachments via REST when online.</summary>
public sealed class ClinicalAttachmentService : IClinicalAttachmentService
{
    private readonly HttpClient _httpClient;
    private readonly ISyncConnectivity _connectivity;

    /// <summary>Uses the host-configured API <see cref="HttpClient"/>.</summary>
    public ClinicalAttachmentService(HttpClient httpClient, ISyncConnectivity connectivity)
    {
        _httpClient = httpClient;
        _connectivity = connectivity;
    }

    public async Task<Result<Guid>> UploadAsync(Guid appointmentId, Stream content, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Result.Failure<Guid>(new Error("Clinical.Offline", "Conecte-se para enviar anexos."));
        }

        using var form = new MultipartFormDataContent();
        form.Add(new StreamContent(content), "file", fileName);
        var response = await _httpClient.PostAsync($"/api/v1/appointments/{appointmentId}/attachments", form, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return Result.Failure<Guid>(new Error("Clinical.UploadFailed", "Falha ao enviar anexo."));
        }

        var id = await response.Content.ReadFromJsonAsync<Guid>(cancellationToken);
        return id == Guid.Empty
            ? Result.Failure<Guid>(new Error("Clinical.UploadFailed", "Resposta inválida do servidor."))
            : Result.Success(id);
    }

    public Task<Result<string>> GetDownloadUrlAsync(Guid attachmentId) =>
        Task.FromResult(Result.Success($"/api/v1/attachments/{attachmentId}/content"));
}
