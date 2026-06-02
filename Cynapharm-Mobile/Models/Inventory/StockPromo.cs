using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Inventory;

/// <summary>
/// Represents one row from GET /stocks-promotionnels/echantillon.
/// The backend returns StockEchantillonDto (inherits StockDelegueDto) with
/// underscore-prefixed field names serialised as camelCase.
/// </summary>
public class StockPromo
{
    [JsonPropertyName("id_stock")]
    public int Id { get; set; }

    [JsonPropertyName("id_Produit")]
    public int ProductId { get; set; }

    /// <summary>
    /// Resolved from ProductService after load — not in the backend DTO.
    /// [JsonPropertyName] is a forward-compat hint.
    /// </summary>
    [JsonPropertyName("nomProduit")]
    public string ProductNom { get; set; } = string.Empty;

    /// <summary>Available quantity — maps from qteDisponible.</summary>
    [JsonPropertyName("qteDisponible")]
    public int Quantite { get; set; }

    // Optional echantillon-specific fields — kept for potential future use
    [JsonPropertyName("qteEchantillon")]
    public int QteEchantillon { get; set; }

    [JsonPropertyName("numeroLot")]
    public string NumeroLot { get; set; } = string.Empty;

    [JsonPropertyName("dateExpiration")]
    public DateTime? DateExpiration { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("qteGratuite")]
    public int QteGratuite { get; set; }

    [JsonPropertyName("typePromotion")]
    public string? TypePromotion { get; set; }

    [JsonPropertyName("quantiteAchat")]
    public int QuantiteAchat { get; set; }

    [JsonPropertyName("quantiteGratuite")]
    public int QuantiteGratuite { get; set; }

    [JsonPropertyName("dateDebut")]
    public DateTime? DateDebut { get; set; }

    [JsonPropertyName("dateFin")]
    public DateTime? DateFin { get; set; }
}
