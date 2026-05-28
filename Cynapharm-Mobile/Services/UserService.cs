using Cynapharm_Mobile.Models.Auth;

namespace Cynapharm_Mobile.Services;

public class UserService
{
    private readonly ApiService _api;
    public UserService(ApiService api) { _api = api; }

    public Task<List<UserListItem>?> GetUsersByRoleAsync(string role)
        => _api.GetAsync<List<UserListItem>>($"auth/users/by-role/{Uri.EscapeDataString(role)}");
}
