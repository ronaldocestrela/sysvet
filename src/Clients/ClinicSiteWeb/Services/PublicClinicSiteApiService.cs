using System.Net.Http.Json;
using Clients.Infrastructure.ClinicSite;
using Core.Domain;

namespace ClinicSiteWeb.Services;

/// <summary>
/// Anonymous HTTP client for public clinic site payloads.
/// </summary>
public sealed class PublicClinicSiteApiService
{
    private readonly IHttpClientFactory _httpClientFactory;

    /// <summary>
    /// Creates the public site API service.
    /// </summary>
    public PublicClinicSiteApiService(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    /// <summary>
    /// Loads published site content for a slug.
    /// </summary>
    public async Task<Result<PublicClinicSiteClientDto>> GetSiteAsync(string slug, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("PublicApi");
        var response = await client.GetAsync($"api/v1/public/clinic-sites/{Uri.EscapeDataString(slug)}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Result.Failure<PublicClinicSiteClientDto>(new Error("ClinicSite.NotFound", "Site not found."));
        }

        if (!response.IsSuccessStatusCode)
        {
            return Result.Failure<PublicClinicSiteClientDto>(new Error("ClinicSite.HttpError", "Unable to load site."));
        }

        var dto = await response.Content.ReadFromJsonAsync<PublicClinicSiteClientDto>(cancellationToken);
        return dto is null
            ? Result.Failure<PublicClinicSiteClientDto>(new Error("ClinicSite.Empty", "Empty response."))
            : Result.Success(dto);
    }
}
