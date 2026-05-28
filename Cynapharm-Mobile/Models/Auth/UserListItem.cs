using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Auth;

public class UserListItem
{
    public int    Id   { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Subtype for CLIENT-role users: "PHARMACIEN" or "GROSSISTE".
    /// Null for other roles (MEDECIN, DELEGUE, …).
    /// </summary>
    [JsonPropertyName("typeClient")]
    public string? TypeClient { get; set; }
}
