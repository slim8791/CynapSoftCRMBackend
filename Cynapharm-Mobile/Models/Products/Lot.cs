using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Products;
public class Lot
{
    [JsonPropertyName("numeroLot")]
    public string NumeroLot { get; set; } = string.Empty;

    [JsonPropertyName("id_Produit")]
    public int IdProduit { get; set; }

    [JsonPropertyName("quantite")]
    public int Quantite { get; set; }

    [JsonPropertyName("dateExpiration")]
    public DateTime DateExpiration { get; set; }

    [JsonPropertyName("dateFabrication")]
    public DateTime? DateFabrication { get; set; }
}
