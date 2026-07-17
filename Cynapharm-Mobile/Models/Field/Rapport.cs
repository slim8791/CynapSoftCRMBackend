using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Field;

public class Rapport
{
    [JsonPropertyName("id_Rapport")]
    public int Id { get; set; }

    [JsonPropertyName("id_Visite")]
    public int VisiteId { get; set; }

    [JsonPropertyName("commentaire")]
    public string Contenu { get; set; } = string.Empty;

    [JsonPropertyName("resultat")]
    public string Resultat { get; set; } = string.Empty;

    [JsonPropertyName("date")]
    public DateTime DateSoumission { get; set; }

    [JsonPropertyName("id_User_Delegue")]
    public int IdDelegue { get; set; }

    [JsonPropertyName("latitude")]
    public double? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double? Longitude { get; set; }

    /// <summary>
    /// JSON-serialized array of { id, nom } objects for products discussed during the visit.
    /// Null when no products were selected.
    /// </summary>
    [JsonPropertyName("produitsDiscutes")]
    public string? ProduitsDiscutes { get; set; }

    [JsonPropertyName("isRejected")]
    public bool IsRejected { get; set; }

    [JsonPropertyName("motifRejet")]
    public string? MotifRejet { get; set; }
}
