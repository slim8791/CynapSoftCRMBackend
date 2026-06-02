namespace Cynapharm_Mobile.Models.Auth;
public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiry { get; set; }
    public UserInfo User { get; set; } = new();
}
