using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Field;

public class Planning
{
    [JsonPropertyName("id_Planning")]
    public int Id { get; set; }

    [JsonPropertyName("id_User_Delegue")]
    public int DelegueId { get; set; }

    // Backend field name is "date"; mobile uses "DatePlanifiee" throughout the ViewModel
    [JsonPropertyName("date")]
    public DateTime DatePlanifiee { get; set; }

    [JsonPropertyName("heureDebut")]
    public TimeSpan HeureDebut { get; set; }

    [JsonPropertyName("heureFin")]
    public TimeSpan HeureFin { get; set; }

    // EtatPlanning: EnAttente=0, Confirme=1, Annule=2
    [JsonPropertyName("etat")]
    public int Etat { get; set; }

    [JsonPropertyName("id_Medecin")]
    public int? IdMedecin { get; set; }

    [JsonPropertyName("id_Pharmacien")]
    public int? IdPharmacien { get; set; }

    // VisiteType: 1=Médecin  2=Pharmacien
    [JsonPropertyName("typeVisite")]
    public int TypeVisite { get; set; } = 1;

    // Resolved locally after load (not from API list)
    [JsonIgnore] public string ContactNom { get; set; } = string.Empty;
    [JsonIgnore] public string TypeLabel  => TypeVisite == 2 ? "Pharmacien" : "Médecin";
    [JsonIgnore] public string EtatLabel  => Etat switch { 1 => "Confirmée", 2 => "Annulée", _ => "Planifiée" };

    public string ClientNom { get; set; } = string.Empty;
    public int? VisiteId { get; set; }

    /// <summary>
    /// Formatted time range for display (e.g., "09:00 – 10:30")
    /// </summary>
    [JsonIgnore]
    public string TimeRange => $"{HeureDebut:hh\\:mm} – {HeureFin:hh\\:mm}";
}
