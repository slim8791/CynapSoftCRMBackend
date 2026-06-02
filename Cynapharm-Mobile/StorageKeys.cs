namespace Cynapharm_Mobile;
public static class StorageKeys
{
    public const string JwtToken    = "jwt_token";
    public const string TokenExpiry = "jwt_expiry";
    public const string UserRole    = "user_role";
    public const string UserId      = "user_id";
    public const string UserName    = "user_name";
    public const string UserEmail   = "user_email";

    public const string UserIdRegion = "user_id_region";

    // Per-user keys — keyed by userId so each account on the device has independent storage.
    public static string UserTelephone(string userId) => $"user_telephone_{userId}";
    public static string UserAdresse(string userId)   => $"user_adresse_{userId}";
}
