namespace Cynapharm_Mobile.Services;

/// <summary>
/// DelegatingHandler that inspects the JWT token before every request.
/// If the token is expired or will expire within 5 minutes, raises
/// <see cref="ApiService.SessionExpired"/> so the app navigates to login
/// proactively rather than hitting a 401 mid-operation.
/// </summary>
public class TokenValidationHandler : DelegatingHandler
{
    private readonly IServiceProvider _services;
    private readonly SemaphoreSlim    _lock = new(1, 1);

    public TokenValidationHandler(IServiceProvider services) => _services = services;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
    {
        var auth = _services.GetRequiredService<AuthService>();

        // Proactive check: if token expires within 5 minutes, logout now
        if (await auth.IsTokenExpiringSoonAsync(TimeSpan.FromMinutes(5)))
        {
            await _lock.WaitAsync(ct);
            try
            {
                // Double-check after acquiring the lock to avoid double-logout
                if (await auth.IsTokenExpiringSoonAsync(TimeSpan.FromMinutes(5)))
                {
                    ApiService.RaiseSessionExpired();
                    // Return a synthetic 401 so callers handle it gracefully
                    return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
                }
            }
            finally { _lock.Release(); }
        }

        var response = await base.SendAsync(request, ct);

        // Also handle unexpected 401 from the server
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            ApiService.RaiseSessionExpired();

        return response;
    }
}
