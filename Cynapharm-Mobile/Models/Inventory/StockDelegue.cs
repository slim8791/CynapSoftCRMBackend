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

    // Display-only — enriched from product catalog, not in backend StockDelegueDto
    public string ProductNom { get; set; } = string.Empty;

    // Kept for offline SQLite compatibility
    public int QuantiteAllouee { get; set; }
}
