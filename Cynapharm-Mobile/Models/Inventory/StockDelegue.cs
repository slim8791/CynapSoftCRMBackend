using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Inventory;

public class StockDelegue
{
    [JsonPropertyName("id_stock")]
    public int Id { get; set; }

    [JsonPropertyName("id_User_Delegue")]
    public int IdDelegue { get; set; }

    [JsonPropertyName("id_Produit")]
    public int ProductId { get; set; }

    [JsonPropertyName("numeroLot")]
    public string NumeroLot { get; set; } = string.Empty;

    [JsonPropertyName("dateExpiration")]
    public DateTime? DateExpiration { get; set; }

    [JsonPropertyName("qteDisponible")]
    public int QuantiteRestante { get; set; }

    [JsonPropertyName("qteReservee")]
    public int QuantiteReservee { get; set; }

    // Enriched from product catalog — backend StockDelegueDto does not include this field;
    // [JsonPropertyName] is kept as a forward-compat hint in case the API adds it later.
    [JsonPropertyName("nomProduit")]
    public string ProductNom { get; set; } = string.Empty;

    // Kept for offline SQLite compatibility
    public int QuantiteAllouee { get; set; }
}
