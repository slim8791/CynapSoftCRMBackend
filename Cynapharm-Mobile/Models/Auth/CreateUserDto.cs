using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Auth;

public class CreateUserDto
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    [JsonPropertyName("phoneNumber")]
    public string? PhoneNumber { get; set; }

    [JsonPropertyName("adresse")]
    public string? Adresse { get; set; }

    [JsonPropertyName("role")]
    public string Role { get; set; } = "CLIENT";

    [JsonPropertyName("userType")]
    public string UserType { get; set; } = "PHARMACIEN";

    [JsonPropertyName("idRegion")]
    public int? IdRegion { get; set; }
}
