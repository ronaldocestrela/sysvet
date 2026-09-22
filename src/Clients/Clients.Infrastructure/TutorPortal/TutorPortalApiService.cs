using Clients.Infrastructure.Http;
using Core.Domain;

namespace Clients.Infrastructure.TutorPortal;

/// <summary>
/// HTTP client for tutor portal authentication and profile APIs.
/// </summary>
public sealed class TutorPortalApiService
{
    private readonly ApiClient _apiClient;

    public TutorPortalApiService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <summary>Self-registers a tutor when CRM email and CPF match.</summary>
    public Task<Result<TutorAuthTokensDto>> RegisterAsync(TutorRegisterRequest request, CancellationToken cancellationToken = default) =>
        _apiClient.PostAsync<TutorRegisterRequest, TutorAuthTokensDto>("/api/v1/tutor-portal/register", request, cancellationToken: cancellationToken);

    /// <summary>Authenticates a tutor portal user.</summary>
    public Task<Result<TutorAuthTokensDto>> LoginAsync(TutorLoginRequest request, CancellationToken cancellationToken = default) =>
        _apiClient.PostAsync<TutorLoginRequest, TutorAuthTokensDto>("/api/v1/tutor-portal/login", request, cancellationToken: cancellationToken);

    /// <summary>Loads the authenticated tutor profile.</summary>
    public Task<Result<TutorPortalMeDto>> GetMeAsync(CancellationToken cancellationToken = default) =>
        _apiClient.GetAsync<TutorPortalMeDto>("/api/v1/tutor-portal/me", cancellationToken);
}

/// <summary>Registration payload for tutor portal.</summary>
public sealed class TutorRegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>Login payload for tutor portal.</summary>
public sealed class TutorLoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>JWT pair returned by tutor portal auth endpoints.</summary>
public sealed class TutorAuthTokensDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresInSeconds { get; set; }
}

/// <summary>Profile returned by tutor portal <c>/me</c>.</summary>
public sealed class TutorPortalMeDto
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public Guid TutorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<TutorPortalPetDto> Pets { get; set; } = [];
}

/// <summary>Pet summary on tutor portal home.</summary>
public sealed class TutorPortalPetDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
