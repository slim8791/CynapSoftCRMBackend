using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Field;

public class Visite
{
    [JsonPropertyName("idVisite")]
    public int Id { get; set; }

    [JsonPropertyName("id_User_Delegue")]
    public int DelegueId { get; set; }

    public string ClientNom { get; set; } = string.Empty;
    public string ClientType { get; set; } = string.Empty;
    public DateTime DateVisite { get; set; }
    public string? Notes { get; set; }
    public bool HasRapport { get; set; }

    // Backend returns IsCompleted (bool); derive Statut for display and local filtering.
    // If Statut is already set explicitly (e.g. during create/update in the ViewModel)
    // the setter will not overwrite it.
    private bool _isCompleted;
    public bool IsCompleted
    {
        get => _isCompleted;
        set
        {
            _isCompleted = value;
            if (string.IsNullOrEmpty(Statut))
                Statut = value ? "REALISEE" : "PLANIFIEE";
        }
    }

    public string Statut { get; set; } = string.Empty;
}
