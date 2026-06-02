using Cynapharm_Mobile.Models.Auth;

namespace Cynapharm_Mobile.Services;

public class UserService
{
    private readonly ApiService _api;
    public UserService(ApiService api) { _api = api; }

    public Task<List<UserListItem>?> GetUsersByRoleAsync(string role)
        => _api.GetAsync<List<UserListItem>>($"auth/users/by-role/{Uri.EscapeDataString(role)}");

    public Task<UserInfo?> GetUserByIdAsync(int id)
        => _api.GetAsync<UserInfo>($"auth/users/{id}");

    public Task<object?> CreateUserAsync(CreateUserDto dto)
        => _api.PostAsync<object>("auth/register", dto);

    public Task<object?> UpdateUserAsync(UpdateUserDto dto)
        => _api.PutAsync<object>("auth/update-profile", dto);
}
