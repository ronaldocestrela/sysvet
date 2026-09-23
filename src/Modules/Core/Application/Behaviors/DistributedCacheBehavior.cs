using System.Text.Json;
using Core.Application.Caching;
using Core.Application.Common.Interfaces;
using Core.Domain;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Core.Application.Behaviors;

/// <summary>
/// Caches successful <see cref="Result{T}"/> responses for queries implementing <see cref="ICacheableQuery"/>.
/// </summary>
public sealed class DistributedCacheBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IDistributedCache _cache;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<DistributedCacheBehavior<TRequest, TResponse>> _logger;

    /// <summary>Creates the behavior.</summary>
    public DistributedCacheBehavior(
        IDistributedCache cache,
        ICurrentUser currentUser,
        ILogger<DistributedCacheBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not ICacheableQuery cacheable)
        {
            return await next();
        }

        var responseType = typeof(TResponse);
        if (!responseType.IsGenericType || responseType.GetGenericTypeDefinition() != typeof(Result<>))
        {
            return await next();
        }

        var valueType = responseType.GetGenericArguments()[0];
        var cacheKey = BuildCacheKey(request, cacheable);

        var cachedBytes = await _cache.GetAsync(cacheKey, cancellationToken);
        if (cachedBytes is { Length: > 0 })
        {
            try
            {
                var cachedValue = JsonSerializer.Deserialize(cachedBytes, valueType, JsonOptions);
                if (cachedValue is not null)
                {
                    _logger.LogDebug("Cache hit for {CacheKey}", cacheKey);
                    return CreateSuccessResult<TResponse>(valueType, cachedValue);
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Ignoring corrupt cache entry {CacheKey}", cacheKey);
            }
        }

        var response = await next();
        if (response.IsSuccess)
        {
            var value = GetResultValue(response, valueType);
            if (value is not null)
            {
                var bytes = JsonSerializer.SerializeToUtf8Bytes(value, valueType, JsonOptions);
                await _cache.SetAsync(
                    cacheKey,
                    bytes,
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = cacheable.CacheDuration },
                    cancellationToken);
            }
        }

        return response;
    }

    private string BuildCacheKey(TRequest request, ICacheableQuery cacheable)
    {
        var scope = request is IPlatformScopedCacheQuery
            ? "platform"
            : _currentUser.TenantId == Guid.Empty
                ? "no-tenant"
                : _currentUser.TenantId.ToString("N");

        var requestName = typeof(TRequest).Name;
        var suffix = string.IsNullOrWhiteSpace(cacheable.CacheKeySuffix) ? "default" : cacheable.CacheKeySuffix;
        return $"sysvet:cache:{scope}:{requestName}:{suffix}";
    }

    private static object? GetResultValue(TResponse response, Type valueType)
    {
        var valueProperty = typeof(Result<>).MakeGenericType(valueType).GetProperty(nameof(Result<object>.Value));
        return valueProperty?.GetValue(response);
    }

    private static TResponse CreateSuccessResult<TResp>(Type valueType, object value)
    {
        var successMethod = typeof(Result)
            .GetMethods()
            .Single(m => m.Name == nameof(Result.Success) && m.IsGenericMethodDefinition && m.GetParameters().Length == 1)
            .MakeGenericMethod(valueType);
        var result = successMethod.Invoke(null, [value])!;
        return (TResponse)result;
    }
}
