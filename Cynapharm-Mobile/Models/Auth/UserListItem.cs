using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Auth;

public class UserListItem
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("phoneNumber")]
    public string? PhoneNumber { get; set; }

    [JsonPropertyName("adresse")]
    public string? Adresse { get; set; }

    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonPropertyName("typeClient")]
    public string? TypeClient { get; set; }

    [JsonPropertyName("isDeleted")]
    public bool IsDeleted { get; set; }

    [JsonPropertyName("idRegion")]
    public int? IdRegion { get; set; }

    [JsonIgnore]
    public string TypeClientLabel => TypeClient ?? Role ?? string.Empty;

    [JsonIgnore]
    public bool IsActive => !IsDeleted;
}
