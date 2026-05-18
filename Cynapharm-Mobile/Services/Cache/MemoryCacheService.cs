namespace Cynapharm_Mobile.Services;

public class MemoryCacheService : ICacheService
{
    private readonly Dictionary<string, CacheEntry> _store = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T?>> factory, TimeSpan ttl)
    {
        await _lock.WaitAsync();
        try
        {
            if (_store.TryGetValue(key, out var hit) && hit.ExpiresAt > DateTime.UtcNow)
                return (T?)hit.Value;
        }
        finally { _lock.Release(); }

        // Run factory outside lock to avoid holding it during network calls
        var result = await factory();

        if (result is not null)
        {
            await _lock.WaitAsync();
            try { _store[key] = new CacheEntry(result, DateTime.UtcNow.Add(ttl)); }
            finally { _lock.Release(); }
        }

        return result;
    }

    public void Invalidate(string key)
    {
        _lock.Wait();
        try { _store.Remove(key); }
        finally { _lock.Release(); }
    }

    public void InvalidateAll()
    {
        _lock.Wait();
        try { _store.Clear(); }
        finally { _lock.Release(); }
    }

    private record CacheEntry(object Value, DateTime ExpiresAt);
}
