using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Orders;
public class Order
{
    [JsonPropertyName("id_Commande")]
    public int Id { get; set; }

    public string NumeroCommande => $"CMD-{Id:D5}";

    public DateTime DateCommande { get; set; }

    // EtatCommande: 0=Brouillon 1=EnAttente 2=Confirmee 3=EnPreparation 4=Expediee 5=Livree 6=Annulee
    public int Statut { get; set; }

    public string StatutFrançais => Statut switch
    {
        0 => "Brouillon",
        1 => "En attente",
        2 => "Confirmée",
        3 => "En préparation",
        4 => "Expédiée",
        5 => "Livrée",
        6 => "Annulée",
        _ => $"Statut {Statut}"
    };

    [JsonPropertyName("montantTotalHT")]
    public decimal MontantTotal { get; set; }

    [JsonPropertyName("montantTTC")]
    public decimal MontantTTC { get; set; }

    [JsonPropertyName("id_Client")]
    public int ClientId { get; set; }

    public string? Notes { get; set; }
    public string? MotifAnnulation { get; set; }
    public bool IsDeleted { get; set; }
    public List<LigneCommande>  Lignes       { get; set; } = new();
    public List<Reclamation>?   Reclamations { get; set; }
}
