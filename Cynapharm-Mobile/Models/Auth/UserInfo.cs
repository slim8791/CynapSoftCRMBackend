using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Auth;

public class UserInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;   // "DELEGUE" | "SUPERVISEUR" | "PHARMACIEN" | ...
    public string? RegionId { get; set; }
    [JsonPropertyName("phoneNumber")]
    public string? Telephone { get; set; }
    public string? Adresse { get; set; }
}
