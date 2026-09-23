using System.Text.Json;
using Core.Application.Behaviors;
using Core.Application.Caching;
using Core.Application.Common.Interfaces;
using Core.Domain;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Core.Tests.Application.Behaviors;

public sealed class DistributedCacheBehaviorTests
{
    private sealed record CacheableQuery(string Suffix) : IRequest<Result<string>>, ICacheableQuery
    {
        public string CacheKeySuffix => Suffix;
        public TimeSpan CacheDuration => TimeSpan.FromMinutes(1);
    }

    [Fact]
    public async Task Handle_OnHit_DoesNotInvokeNext()
    {
        var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        var currentUser = Substitute.For<ICurrentUser>();
        var tenantId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        currentUser.TenantId.Returns(tenantId);

        var behavior = new DistributedCacheBehavior<CacheableQuery, Result<string>>(
            cache,
            currentUser,
            NullLogger<DistributedCacheBehavior<CacheableQuery, Result<string>>>.Instance);

        await cache.SetAsync(
            $"sysvet:cache:{tenantId:N}:CacheableQuery:hit",
            JsonSerializer.SerializeToUtf8Bytes("cached"),
            new DistributedCacheEntryOptions());

        var next = Substitute.For<RequestHandlerDelegate<Result<string>>>();
        next.Invoke().Returns(Task.FromResult(Result.Success("fresh")));

        var result = await behavior.Handle(new CacheableQuery("hit"), next, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("cached", result.Value);
        await next.DidNotReceive().Invoke();
    }

    [Fact]
    public async Task Handle_OnMiss_StoresSuccessfulResult()
    {
        var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.TenantId.Returns(Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"));

        var behavior = new DistributedCacheBehavior<CacheableQuery, Result<string>>(
            cache,
            currentUser,
            NullLogger<DistributedCacheBehavior<CacheableQuery, Result<string>>>.Instance);

        var next = Substitute.For<RequestHandlerDelegate<Result<string>>>();
        next.Invoke().Returns(Task.FromResult(Result.Success("stored")));

        var first = await behavior.Handle(new CacheableQuery("miss"), next, CancellationToken.None);
        Assert.Equal("stored", first.Value);

        next.ClearReceivedCalls();
        var second = await behavior.Handle(new CacheableQuery("miss"), next, CancellationToken.None);
        Assert.Equal("stored", second.Value);
        await next.DidNotReceive().Invoke();
    }

    [Fact]
    public async Task Handle_Failure_IsNotCached()
    {
        var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.TenantId.Returns(Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"));

        var behavior = new DistributedCacheBehavior<CacheableQuery, Result<string>>(
            cache,
            currentUser,
            NullLogger<DistributedCacheBehavior<CacheableQuery, Result<string>>>.Instance);

        var next = Substitute.For<RequestHandlerDelegate<Result<string>>>();
        next.Invoke().Returns(Task.FromResult(Result.Failure<string>(ErrorCodes.Validation.Error)));

        await behavior.Handle(new CacheableQuery("fail"), next, CancellationToken.None);
        var cached = await cache.GetStringAsync("sysvet:cache:aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee:CacheableQuery:fail");
        Assert.Null(cached);
    }
}
