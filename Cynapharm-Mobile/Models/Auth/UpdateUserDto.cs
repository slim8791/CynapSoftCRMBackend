using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Auth;

/// <summary>
/// DTO for updating a user's profile fields.
/// Maps to backend's UpdateProfileDto (PUT /auth/update-profile).
/// </summary>
public class UpdateUserDto
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("phoneNumber")]
    public string? PhoneNumber { get; set; }

    [JsonPropertyName("adresse")]
    public string? Adresse { get; set; }
}
