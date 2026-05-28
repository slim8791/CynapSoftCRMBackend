using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Field;

public class Visite
{
    [JsonPropertyName("idVisite")]
    public int Id { get; set; }

    [JsonPropertyName("dateVisite")]
    public DateTime DateVisite { get; set; }

    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonIgnore]
    public string TypeLabel => Type switch
    {
        1 => "Médecin",
        2 => "Pharmacien",
        3 => "Autre",
        _ => $"Type {Type}"
    };

    [JsonPropertyName("isCompleted")]
    public bool IsCompleted { get; set; }

    [JsonPropertyName("id_User_Delegue")]
    public int IdDelegue { get; set; }

    [JsonPropertyName("idMedecin")]
    public int? IdMedecin { get; set; }

    [JsonPropertyName("idPharmacien")]
    public int? IdPharmacien { get; set; }

    [JsonPropertyName("idPlanning")]
    public int? IdPlanning { get; set; }

    [JsonPropertyName("hasRapport")]
    public bool HasRapport { get; set; }

    [JsonIgnore]
    public string Statut => IsCompleted ? "Complétée" : "Non complétée";

    // ── Resolved display names (populated by ViewModel after load, never from API)
    [JsonIgnore]
    public string MedecinNom { get; set; } = string.Empty;

    [JsonIgnore]
    public string PharmacienNom { get; set; } = string.Empty;

    [JsonIgnore]
    public string ContactName =>
        !string.IsNullOrEmpty(MedecinNom)
            ? MedecinNom
            : !string.IsNullOrEmpty(PharmacienNom)
                ? PharmacienNom
                : string.Empty;

    [JsonIgnore]
    public bool HasContact => !string.IsNullOrEmpty(ContactName);
}
