using Cynapharm_Mobile.Services;

namespace Cynapharm_Mobile.Tests.Services;

public class MemoryCacheServiceTests
{
    private static MemoryCacheService Create() => new();

    // ── Cache miss ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetOrCreateAsync_InvokesFactory_OnCacheMiss()
    {
        var cache = Create();
        var callCount = 0;

        await cache.GetOrCreateAsync("key", () =>
        {
            callCount++;
            return Task.FromResult<string?>("value");
        }, TimeSpan.FromMinutes(5));

        Assert.Equal(1, callCount);
    }

    // ── Cache hit ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetOrCreateAsync_ReturnsCachedValue_WithoutCallingFactory_OnCacheHit()
    {
        var cache = Create();
        var callCount = 0;
        var factory = () => { callCount++; return Task.FromResult<string?>("v"); };

        await cache.GetOrCreateAsync("key", factory, TimeSpan.FromMinutes(5));
        var result = await cache.GetOrCreateAsync("key", factory, TimeSpan.FromMinutes(5));

        Assert.Equal("v", result);
        Assert.Equal(1, callCount);
    }

    // ── TTL expiry ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetOrCreateAsync_InvokesFactory_AfterTtlExpires()
    {
        var cache = Create();
        var callCount = 0;
        var factory = () => { callCount++; return Task.FromResult<string?>("v"); };

        await cache.GetOrCreateAsync("key", factory, TimeSpan.FromMilliseconds(30));
        await Task.Delay(80); // wait for TTL to expire
        await cache.GetOrCreateAsync("key", factory, TimeSpan.FromMilliseconds(30));

        Assert.Equal(2, callCount);
    }

    // ── Null result not cached ────────────────────────────────────────────────

    [Fact]
    public async Task GetOrCreateAsync_DoesNotCacheNullResult()
    {
        var cache = Create();
        var callCount = 0;

        await cache.GetOrCreateAsync<string>("key", () =>
        {
            callCount++;
            return Task.FromResult<string?>(null);
        }, TimeSpan.FromMinutes(5));

        await cache.GetOrCreateAsync<string>("key", () =>
        {
            callCount++;
            return Task.FromResult<string?>(null);
        }, TimeSpan.FromMinutes(5));

        Assert.Equal(2, callCount);
    }

    // ── Invalidate ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Invalidate_RemovesEntry_SoNextCallInvokesFactory()
    {
        var cache = Create();
        var callCount = 0;
        var factory = () => { callCount++; return Task.FromResult<string?>("v"); };

        await cache.GetOrCreateAsync("key", factory, TimeSpan.FromMinutes(5));
        cache.Invalidate("key");
        await cache.GetOrCreateAsync("key", factory, TimeSpan.FromMinutes(5));

        Assert.Equal(2, callCount);
    }

    [Fact]
    public async Task InvalidateAll_ClearsAllEntries_SoSubsequentCallsInvokeFactory()
    {
        var cache = Create();
        var calls = new Dictionary<string, int> { ["a"] = 0, ["b"] = 0 };

        await cache.GetOrCreateAsync("a", () => { calls["a"]++; return Task.FromResult<string?>("a"); }, TimeSpan.FromMinutes(5));
        await cache.GetOrCreateAsync("b", () => { calls["b"]++; return Task.FromResult<string?>("b"); }, TimeSpan.FromMinutes(5));

        cache.InvalidateAll();

        await cache.GetOrCreateAsync("a", () => { calls["a"]++; return Task.FromResult<string?>("a"); }, TimeSpan.FromMinutes(5));
        await cache.GetOrCreateAsync("b", () => { calls["b"]++; return Task.FromResult<string?>("b"); }, TimeSpan.FromMinutes(5));

        Assert.Equal(2, calls["a"]);
        Assert.Equal(2, calls["b"]);
    }

    // ── Key isolation ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetOrCreateAsync_ReturnsDifferentValues_ForDifferentKeys()
    {
        var cache = Create();

        var r1 = await cache.GetOrCreateAsync("k1", () => Task.FromResult<string?>("one"), TimeSpan.FromMinutes(5));
        var r2 = await cache.GetOrCreateAsync("k2", () => Task.FromResult<string?>("two"), TimeSpan.FromMinutes(5));

        Assert.Equal("one", r1);
        Assert.Equal("two", r2);
    }
}
