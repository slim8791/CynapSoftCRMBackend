using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Orders;

public class Reclamation
{
    [JsonPropertyName("id_Rec")]
    public int Id { get; set; }

    [JsonPropertyName("id_Commande")]
    public int CommandeId { get; set; }

    [JsonPropertyName("id_Ligne")]
    public int LigneId { get; set; }

    [JsonPropertyName("message")]
    public string Motif { get; set; } = string.Empty;

    [JsonPropertyName("dateReclamation")]
    public DateTime DateCreation { get; set; }

    [JsonPropertyName("statut")]
    public int Statut { get; set; } = 0;

    // ── Computed properties for display ──
    public string StatutLabel => Statut switch
    {
        0 => "Ouverte",
        1 => "En cours",
        2 => "Résolue",
        3 => "Rejetée",
        _ => "Inconnu"
    };

    public string StatutColor => Statut switch
    {
        0 => "#F59E0B",      // Amber
        1 => "#0077B6",      // Blue
        2 => "#10B981",      // Green
        3 => "#EF4444",      // Red
        _ => "#64748B"       // Slate
    };
}
