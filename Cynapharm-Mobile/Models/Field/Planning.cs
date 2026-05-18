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

    public TimeSpan HeureDebut { get; set; }
    public TimeSpan HeureFin { get; set; }

    // Etat is an enum on the backend (e.g. PLANIFIE, VALIDE); stored as string here
    public string Etat { get; set; } = string.Empty;

    // These fields don't come from the backend list response but are kept
    // for local use (e.g. when creating a new planning entry)
    public string ClientNom { get; set; } = string.Empty;
    public string? Objectif { get; set; }
    public int? VisiteId { get; set; }
}
