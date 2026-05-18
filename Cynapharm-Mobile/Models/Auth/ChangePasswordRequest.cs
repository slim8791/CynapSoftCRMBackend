namespace Cynapharm_Mobile.Models.Auth;
public record ChangePasswordRequest(string Email, string CurrentPassword, string NewPassword);
