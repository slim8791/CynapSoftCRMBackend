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

    public string? Statut { get; set; }
}
