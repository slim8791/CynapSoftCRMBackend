# Login Connection Error - Fix Applied

## Problem Identified ❌

When running the mobile app on Windows desktop and attempting to login, the app displayed:
> **"Erreur de connexion. Vérifiez votre connexion internet."** (Connection error. Check your internet connection.)

### Root Cause

The MAUI app was trying to connect to the **wrong HTTPS port**:
- **App was trying:** `https://localhost:5555/`
- **Backend actually runs on:** `https://localhost:7777/` (HTTPS) or `http://localhost:5555/` (HTTP)

The backend configuration shows:
```json
"https": {
  "applicationUrl": "https://localhost:7777;http://localhost:5555"
}
```

**Port 5555** is HTTP-only, not HTTPS! This caused the SSL handshake to fail.

---

## Solution Applied ✅

### 1. **Fixed Base URL in MauiProgram.cs**
Changed the `GetApiBaseUrl()` method to use the correct HTTPS port:

```csharp
private static string GetApiBaseUrl()
{
#if ANDROID
    return "https://10.0.2.2:7777/";  // Changed from 5555 to 7777
#elif IOS
    return "https://localhost:7777/";  // Changed from 5555 to 7777
#else
    // Windows desktop - use HTTPS port 7777
    return "https://localhost:7777/";  // Changed from 5555 to 7777
#endif
}
```

### 2. **Improved HttpClientHandler Configuration**
Enhanced the SSL bypass handler with additional settings for Windows:

```csharp
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true,
    AllowAutoRedirect = true,
    UseCookies = true
});
```

Added:
- `AllowAutoRedirect = true` - Handles redirects properly
- `UseCookies = true` - Supports cookie-based operations

### 3. **Enhanced Error Logging in ApiService.cs**
Added try-catch blocks and debug output to diagnose connection issues:

```csharp
public async Task<T?> PostAsync<T>(string endpoint, object payload, CancellationToken ct = default)
{
    try
    {
        await AttachTokenAsync();
        var response = await _http.PostAsJsonAsync(endpoint, payload, _jsonOptions, ct);
        return await ReadAsync<T>(response, ct);
    }
    catch (HttpRequestException ex)
    {
        Debug.WriteLine($"[API POST ERROR] {endpoint}: {ex.Message}");
        throw;
    }
}
```

This will now show detailed error messages in the Debug Output window when requests fail.

---

## Testing Steps

1. **Ensure backend is running:**
   ```powershell
   # Run the API Gateway with HTTPS profile
   cd CynapCRM.Gateway
   dotnet run --launch-profile https
   ```
   Verify it's running on `https://localhost:7777`

2. **Run the mobile app on Windows:**
   ```
   Run the app (Ctrl+F5)
   ```

3. **Test login:**
   - Enter valid credentials
   - You should now successfully connect to the backend
   - If still failing, check Debug Output for error messages

4. **Check Debug Output:**
   - Open: `Debug` → `Windows` → `Output`
   - Filter to see `[API POST ERROR]` messages for detailed error info

---

## Configuration Summary

| Platform | Base URL | Port | Protocol |
|----------|----------|------|----------|
| **Android Emulator** | `https://10.0.2.2:7777/` | 7777 | HTTPS |
| **iOS Simulator** | `https://localhost:7777/` | 7777 | HTTPS |
| **Windows Desktop** | `https://localhost:7777/` | 7777 | HTTPS |
| **Backend (HTTPS)** | `https://localhost:7777/` | 7777 | HTTPS |
| **Backend (HTTP)** | `http://localhost:5555/` | 5555 | HTTP |

---

## Notes

- SSL certificate validation bypass is **only enabled in DEBUG mode**
- Release builds will enforce strict certificate validation (production certificates required)
- All platforms now use the same HTTPS port (7777) for consistency
- Enhanced logging helps diagnose any future connection issues
