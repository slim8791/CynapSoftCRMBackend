using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Inventory;

/// <summary>
/// A sample (échantillon) distributed by a delegate to the médecin.
/// Maps the InventoryAPI EchantillonDto returned by distributions/by-medecin/{id}.
/// </summary>
public class EchantillonRecu
{
    [JsonPropertyName("id_Distribution")]
    public int Id { get; set; }

    [JsonPropertyName("id_Delegue")]
    public int IdDelegue { get; set; }

    [JsonPropertyName("id_Medecin")]
    public int? IdMedecin { get; set; }

    [JsonPropertyName("id_Stock")]
    public int IdStock { get; set; }

    [JsonPropertyName("id_Produit")]
    public int IdProduit { get; set; }

    [JsonPropertyName("qte")]
    public int Quantite { get; set; }

    [JsonPropertyName("numeroLot")]
    public string NumeroLot { get; set; } = string.Empty;

    [JsonPropertyName("dateDistribution")]
    public DateTime DateDistribution { get; set; }

    // Resolved product name — populated by the ViewModel after load (never from API).
    [JsonIgnore]
    public string ProduitNom { get; set; } = string.Empty;

    [JsonIgnore]
    public string ProduitLabel =>
        string.IsNullOrWhiteSpace(ProduitNom) ? $"Produit #{IdProduit}" : ProduitNom;

    [JsonIgnore]
    public string Display => $"{ProduitLabel} × {Quantite}";
}
