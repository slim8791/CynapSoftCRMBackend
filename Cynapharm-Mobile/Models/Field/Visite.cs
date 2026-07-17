using System.Text.Json.Serialization;
using Cynapharm_Mobile.Models.Inventory;

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

    [JsonPropertyName("isRejected")]
    public bool IsRejected { get; set; }

    [JsonIgnore]
    public string Statut
    {
        get
        {
            if (IsCompleted) return "Validée";
            if (IsRejected) return "Rejetée";
            if (HasRapport) return "En attente";
            if (IsStarted) return "En cours";
            return "Planifiée";
        }
    }

    // ── Resolved display names (populated by ViewModel after load, never from API)
    [JsonIgnore]
    public string MedecinNom { get; set; } = string.Empty;

    [JsonIgnore]
    public string PharmacienNom { get; set; } = string.Empty;

    // Resolved delegate name — used by the médecin's visit-history view
    [JsonIgnore]
    public string DelegueNom { get; set; } = string.Empty;

    [JsonIgnore]
    public bool HasDelegue => !string.IsNullOrEmpty(DelegueNom);

    [JsonIgnore]
    public string DelegueLabel => HasDelegue ? DelegueNom : $"Délégué #{IdDelegue}";

    // ── Products discussed during the visit (from the linked rapport) ──────────
    // Raw JSON [{ "id", "nom" }] from the backend; parsed names filled by the ViewModel.
    [JsonPropertyName("produitsDiscutes")]
    public string? ProduitsDiscutes { get; set; }

    [JsonIgnore]
    public List<string> ProduitsDiscutesNoms { get; set; } = new();

    [JsonIgnore]
    public bool HasProduitsDiscutes => ProduitsDiscutesNoms.Count > 0;

    // ── Samples received during the visit (resolved by the ViewModel) ──────────
    [JsonIgnore]
    public List<EchantillonRecu> EchantillonsRecus { get; set; } = new();

    [JsonIgnore]
    public bool HasEchantillons => EchantillonsRecus.Count > 0;

    [JsonIgnore]
    public string ContactName =>
        !string.IsNullOrEmpty(MedecinNom)
            ? MedecinNom
            : !string.IsNullOrEmpty(PharmacienNom)
                ? PharmacienNom
                : string.Empty;

    [JsonPropertyName("heureDebut")]
    public DateTime? HeureDebut { get; set; }

    [JsonPropertyName("isStarted")]
    public bool IsStarted { get; set; }

    [JsonIgnore]
    public bool CanStart => !IsStarted && !IsCompleted;

    [JsonIgnore]
    public string HeurDebutLabel =>
        HeureDebut?.Year > 1
            ? HeureDebut.Value.ToLocalTime().ToString("HH:mm")
            : string.Empty;

    [JsonIgnore]
    public bool HasContact => !string.IsNullOrEmpty(ContactName);
}
