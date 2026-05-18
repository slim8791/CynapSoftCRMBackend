using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Orders;
public class Order
{
    [JsonPropertyName("id_Commande")]
    public int Id { get; set; }

    public string NumeroCommande => $"CMD-{Id:D5}";

    public DateTime DateCommande { get; set; }
    public string Statut { get; set; } = string.Empty;

    [JsonPropertyName("montantTotalHT")]
    public decimal MontantTotal { get; set; }

    [JsonPropertyName("id_Client")]
    public int ClientId { get; set; }

    public string? Notes { get; set; }
    public string? MotifAnnulation { get; set; }
    public bool IsDeleted { get; set; }
    public List<LigneCommande>  Lignes       { get; set; } = new();
    public List<Reclamation>?   Reclamations { get; set; }
}
