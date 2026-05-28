using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Field;

public class Region
{
    [JsonPropertyName("id_Region")]
    public int Id { get; set; }

    [JsonPropertyName("nomRegion")]
    public string Nom { get; set; } = string.Empty;

    [JsonPropertyName("codePostal")]
    public string CodePostal { get; set; } = string.Empty;

    [JsonPropertyName("id_User_Delegue")]
    public int IdDelegue { get; set; }
}
