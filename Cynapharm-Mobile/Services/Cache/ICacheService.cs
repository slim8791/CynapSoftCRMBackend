namespace Cynapharm_Mobile.Services;

public interface ICacheService
{
    /// <summary>
    /// Returns the cached value for <paramref name="key"/> if still valid,
    /// otherwise calls <paramref name="factory"/>, caches the result with <paramref name="ttl"/>, and returns it.
    /// </summary>
    Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T?>> factory, TimeSpan ttl);

    /// <summary>Removes a single cache entry.</summary>
    void Invalidate(string key);

    /// <summary>Clears all entries (call on logout).</summary>
    void InvalidateAll();
}
