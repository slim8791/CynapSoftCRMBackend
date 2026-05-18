using System.IdentityModel.Tokens.Jwt;
using Cynapharm_Mobile.Models.Auth;
using Cynapharm_Mobile.Models.Common;

namespace Cynapharm_Mobile.Services;

public class AuthService
{
    private readonly ApiService _api;

    public AuthService(ApiService api)
    {
        _api = api;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var result = await _api.PostAsync<LoginResponse>("auth/login", request);
        if (result != null && result.User != null)
        {
            await SecureStorage.SetAsync(StorageKeys.JwtToken, result.Token);

            // Calculate expiry from JWT since backend doesn't send it in LoginResponseDto
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(result.Token);
                await SecureStorage.SetAsync(StorageKeys.TokenExpiry, jwt.ValidTo.ToString("O"));
            }
            catch { }

            var userId = result.User.Id.ToString();
            await SecureStorage.SetAsync(StorageKeys.UserRole,  result.User.Role);
            await SecureStorage.SetAsync(StorageKeys.UserId,    userId);
            await SecureStorage.SetAsync(StorageKeys.UserName,  result.User.Name);
            await SecureStorage.SetAsync(StorageKeys.UserEmail, result.User.Email);

            // Phone and address stored per-user so different accounts on the same
            // device never share each other's data.
            await SecureStorage.SetAsync(StorageKeys.UserTelephone(userId), result.User.Telephone ?? string.Empty);
            await SecureStorage.SetAsync(StorageKeys.UserAdresse(userId),   result.User.Adresse   ?? string.Empty);
        }
        return result;
    }

    public async Task ForgotPasswordAsync(string email)
        => await _api.PostAsync<object>("auth/forgot-password", new { Email = email });

    public async Task ChangePasswordAsync(ChangePasswordRequest request)
        => await _api.PutAsync<object>("auth/change-password", request);

    public async Task<UserInfo?> GetCurrentUserAsync()
    {
        var name  = await SecureStorage.GetAsync(StorageKeys.UserName)  ?? string.Empty;
        var email = await SecureStorage.GetAsync(StorageKeys.UserEmail) ?? string.Empty;
        var role  = await SecureStorage.GetAsync(StorageKeys.UserRole)  ?? string.Empty;
        var idStr = await SecureStorage.GetAsync(StorageKeys.UserId)    ?? "0";

        // Fallback: extract email from JWT claims if not yet persisted in SecureStorage
        if (string.IsNullOrEmpty(email))
        {
            try
            {
                var token = await SecureStorage.GetAsync(StorageKeys.JwtToken);
                if (!string.IsNullOrEmpty(token))
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwt = handler.ReadJwtToken(token);
                    email = jwt.Claims
                        .FirstOrDefault(c => c.Type is "email"
                            or System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)
                        ?.Value ?? string.Empty;
                }
            }
            catch { }
        }

        var telephone = await SecureStorage.GetAsync(StorageKeys.UserTelephone(idStr)) ?? string.Empty;
        var adresse   = await SecureStorage.GetAsync(StorageKeys.UserAdresse(idStr))   ?? string.Empty;

        return new UserInfo
        {
            Id        = int.TryParse(idStr, out var id) ? id : 0,
            Name      = name,
            Email     = email,
            Role      = role,
            Telephone = telephone,
            Adresse   = adresse,
        };
    }

    public void Logout()
    {
        SecureStorage.Remove(StorageKeys.JwtToken);
        SecureStorage.Remove(StorageKeys.TokenExpiry);
        SecureStorage.Remove(StorageKeys.UserRole);
        SecureStorage.Remove(StorageKeys.UserId);
        SecureStorage.Remove(StorageKeys.UserName);
        SecureStorage.Remove(StorageKeys.UserEmail);
        _api.ClearAuthorizationHeader();
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        try
        {
            var token = await SecureStorage.GetAsync(StorageKeys.JwtToken);
            if (string.IsNullOrEmpty(token)) return false;
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            return jwt.ValidTo > DateTime.UtcNow;
        }
        catch { return false; }
    }

    public async Task<string> GetUserRoleAsync()
        => await SecureStorage.GetAsync(StorageKeys.UserRole) ?? string.Empty;

    /// <summary>
    /// Returns true if the stored JWT is expired or will expire within <paramref name="threshold"/>.
    /// Used by <see cref="TokenValidationHandler"/> to raise a proactive session-expired event.
    /// </summary>
    public async Task<bool> IsTokenExpiringSoonAsync(TimeSpan threshold)
    {
        try
        {
            var token = await SecureStorage.GetAsync(StorageKeys.JwtToken);
            if (string.IsNullOrEmpty(token)) return false; // no token = not authenticated, not "expiring"
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            return jwt.ValidTo <= DateTime.UtcNow.Add(threshold);
        }
        catch { return false; }
    }
}
