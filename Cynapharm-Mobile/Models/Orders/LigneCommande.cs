using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Orders;

public class LigneCommande
{
    [JsonPropertyName("id_Ligne")]
    public int Id { get; set; }

    [JsonPropertyName("id_Commande")]
    public int CommandeId { get; set; }

    [JsonPropertyName("id_Produit")]
    public int ProductId { get; set; }

    public string ProductNom { get; set; } = string.Empty;

    [JsonIgnore]
    public string DisplayName => string.IsNullOrEmpty(ProductNom) ? $"Produit #{ProductId}" : ProductNom;

    public int Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }
    public string NumeroLot { get; set; } = string.Empty;
    public decimal Remise { get; set; }

    [JsonIgnore]
    public decimal SousTotal => Quantite * PrixUnitaire * (1m - Remise / 100m);
}
