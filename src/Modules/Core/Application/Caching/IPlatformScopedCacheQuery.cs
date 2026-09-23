namespace Core.Application.Caching;

/// <summary>
/// Cache scope is global platform (Super Admin), not tenant-isolated.</summary>
public interface IPlatformScopedCacheQuery : ICacheableQuery;
