using System.Diagnostics;

namespace Cynapharm_Mobile.Services;

public class HttpLoggingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        Debug.WriteLine($"[HTTP →] {request.Method} {request.RequestUri}");

        HttpResponseMessage response;
        try
        {
            response = await base.SendAsync(request, ct);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[HTTP ✗] {request.RequestUri} — {ex.Message} ({sw.ElapsedMilliseconds}ms)");
            throw;
        }

        sw.Stop();
        var icon = response.IsSuccessStatusCode ? "✓" : "✗";
        Debug.WriteLine($"[HTTP {icon}] {(int)response.StatusCode} {request.RequestUri} ({sw.ElapsedMilliseconds}ms)");
        return response;
    }
}
