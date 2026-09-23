using Core.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Platform.Application.Abstractions;

namespace Platform.Infrastructure.Auditing;

/// <summary>Resolves Super Admin operator and IP from the current HTTP request (9.7).</summary>
public sealed class HttpPlatformAuditContext : IPlatformAuditContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICurrentUser _currentUser;

    /// <summary>Creates the context.</summary>
    public HttpPlatformAuditContext(IHttpContextAccessor httpContextAccessor, ICurrentUser currentUser)
    {
        _httpContextAccessor = httpContextAccessor;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public string ActorUserId =>
        string.IsNullOrWhiteSpace(_currentUser.UserId) ? "unknown" : _currentUser.UserId;

    /// <inheritdoc />
    public string ClientIp =>
        _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}
