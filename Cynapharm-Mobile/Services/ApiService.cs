using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Cynapharm_Mobile.Models.Common;

namespace Cynapharm_Mobile.Services;

public class ApiService
{
    protected readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static event EventHandler? SessionExpired;

    internal static void RaiseSessionExpired()
        => SessionExpired?.Invoke(null, EventArgs.Empty);

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public void ClearAuthorizationHeader()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    private Uri BuildUri(string endpoint)
    {
        if (string.IsNullOrEmpty(endpoint))
            throw new ArgumentNullException(nameof(endpoint), "Endpoint cannot be null or empty");
        
        if (_httpClient.BaseAddress == null)
            throw new InvalidOperationException("HttpClient BaseAddress is not configured.");
        
        var normalizedEndpoint = endpoint.StartsWith('/') ? endpoint : $"/{endpoint}";
        return new Uri(_httpClient.BaseAddress, normalizedEndpoint);
    }

    private async Task PrepareAuthHeaderAsync()
    {
        var token = await SecureStorage.GetAsync(StorageKeys.JwtToken);
        if (!string.IsNullOrEmpty(token))
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<T?> GetAsync<T>(string endpoint, CancellationToken ct = default)
    {
        try
        {
            await PrepareAuthHeaderAsync();
            var uri = BuildUri(endpoint);
            var response = await _httpClient.GetAsync(uri, ct);
            return await HandleResponseAsync<T>(response);
        }
        catch (TaskCanceledException)
        {
            throw new ApiException("La requête a expiré. Vérifiez votre connexion internet.");
        }
        catch (Exception ex) when (ex is not ApiException)
        {
            System.Diagnostics.Debug.WriteLine($"GetAsync Error: {ex}");
            throw;
        }
    }

    public async Task<T?> PostAsync<T>(string endpoint, object payload, CancellationToken ct = default)
    {
        try
        {
            await PrepareAuthHeaderAsync();
            var uri = BuildUri(endpoint);
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(uri, content, ct);
            return await HandleResponseAsync<T>(response);
        }
        catch (TaskCanceledException)
        {
            throw new ApiException("La requête a expiré. Vérifiez votre connexion internet.");
        }
        catch (Exception ex) when (ex is not ApiException)
        {
            System.Diagnostics.Debug.WriteLine($"PostAsync Error: {ex}");
            throw;
        }
    }

    public async Task<T?> PutAsync<T>(string endpoint, object payload, CancellationToken ct = default)
    {
        try
        {
            await PrepareAuthHeaderAsync();
            var uri = BuildUri(endpoint);
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(uri, content, ct);
            return await HandleResponseAsync<T>(response);
        }
        catch (TaskCanceledException)
        {
            throw new ApiException("La requête a expiré. Vérifiez votre connexion internet.");
        }
        catch (Exception ex) when (ex is not ApiException)
        {
            System.Diagnostics.Debug.WriteLine($"PutAsync Error: {ex}");
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string endpoint, CancellationToken ct = default)
    {
        try
        {
            await PrepareAuthHeaderAsync();
            var uri = BuildUri(endpoint);
            var response = await _httpClient.DeleteAsync(uri, ct);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                HandleUnauthorized();
                return false;
            }
            return response.IsSuccessStatusCode;
        }
        catch (TaskCanceledException)
        {
            throw new ApiException("La requête a expiré. Vérifiez votre connexion internet.");
        }
        catch (Exception ex) when (ex is not ApiException)
        {
            System.Diagnostics.Debug.WriteLine($"DeleteAsync Error: {ex}");
            throw;
        }
    }

    private async Task<T?> HandleResponseAsync<T>(HttpResponseMessage response)
    {
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            HandleUnauthorized();
            return default;
        }
        
        if (!response.IsSuccessStatusCode)
        {
            var message = response.StatusCode switch
            {
                System.Net.HttpStatusCode.BadRequest          => "Requête invalide.",
                System.Net.HttpStatusCode.Unauthorized        => "Authentification requise.",
                System.Net.HttpStatusCode.Forbidden           => "Accès refusé. Vous n'avez pas les droits pour cette action.",
                System.Net.HttpStatusCode.NotFound            => "Ressource introuvable.",
                System.Net.HttpStatusCode.InternalServerError => "Erreur serveur. Veuillez réessayer.",
                System.Net.HttpStatusCode.ServiceUnavailable  => "Service temporairement indisponible. Réessayez dans quelques instants.",
                System.Net.HttpStatusCode.RequestTimeout      => "La requête a expiré. Vérifiez votre connexion.",
                _                                             => $"Erreur de communication ({(int)response.StatusCode})."
            };
            throw new ApiException(message, null, response.StatusCode);
        }

        var json = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(json)) return default;

        // Try wrapped ResponseDto first ({"Result":...,"IsSuccess":true})
        // Fall back to direct deserialization if the JSON doesn't look like a wrapper
        try
        {
            var wrapper = JsonSerializer.Deserialize<ApiResponse<T>>(json, _jsonOptions);
            if (wrapper != null && (wrapper.Result != null || json.Contains("\"IsSuccess\"", StringComparison.OrdinalIgnoreCase)))
            {
                if (!wrapper.IsSuccess)
                    throw new HttpRequestException(wrapper.Message ?? "Erreur du serveur.");
                return wrapper.Result;
            }
        }
        catch (HttpRequestException) { throw; }
        catch { /* not a wrapped response — fall through to direct deserialization */ }

        // Direct deserialization for endpoints that return plain JSON
        return JsonSerializer.Deserialize<T>(json, _jsonOptions);
    }

    // Reused across calls — avoids socket exhaustion for external (CDN) downloads.
    private static readonly HttpClient _externalClient = new();

    /// <summary>
    /// Downloads raw bytes from any URL.
    /// Sends the Bearer token only when the URL targets our own API host;
    /// external CDN URLs (Cloudinary, S3, …) are fetched without auth headers.
    /// </summary>
    public async Task<byte[]?> DownloadFileAsync(string url)
    {
        var uri = new Uri(url);
        bool isOwnApi = uri.Host.Equals(
            _httpClient.BaseAddress?.Host, StringComparison.OrdinalIgnoreCase);

        HttpResponseMessage response;
        if (isOwnApi)
        {
            await PrepareAuthHeaderAsync();
            response = await _httpClient.GetAsync(url);
        }
        else
        {
            response = await _externalClient.GetAsync(url);
        }

        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadAsByteArrayAsync();
    }

    private static void HandleUnauthorized()
    {
        SecureStorage.Remove(StorageKeys.JwtToken);
        RaiseSessionExpired();
    }
}
