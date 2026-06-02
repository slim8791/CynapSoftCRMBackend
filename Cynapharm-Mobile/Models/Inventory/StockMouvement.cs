using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Inventory;

public class StockMouvement
{
    // ── Backend fields ────────────────────────────────────────────────────────
    // The backend StockMovementDto uses English "Movement" (not French "Mouvement")
    // — [JsonPropertyName] is mandatory here to bridge that difference.

    [JsonPropertyName("id_Movement")]
    public int Id { get; set; }

    /// <summary>Id of the StockDelegue row this movement belongs to.</summary>
    [JsonPropertyName("id_Stock")]
    public int IdStock { get; set; }

    [JsonPropertyName("quantite")]
    public int Quantite { get; set; }

    /// <summary>Backend values: "Increment", "Decrement", "Transfer-In", "Transfer-Out".</summary>
    [JsonPropertyName("typeMovement")]
    public string TypeMouvement { get; set; } = string.Empty;

    [JsonPropertyName("dateMovement")]
    public DateTime DateMouvement { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    // ── Display-only — resolved after loading, not in the API response ────────

    /// <summary>
    /// Filled in MyStockViewModel after load, by matching IdStock against the
    /// echantillon list (or ProductService for unmatched stocks).
    /// </summary>
    public string ProductNom { get; set; } = string.Empty;

    // ── Computed helpers ──────────────────────────────────────────────────────

    [JsonIgnore]
    public bool IsPositive => Quantite >= 0;

    /// <summary>Shows signed quantity: "+5" for additions, "-3" for deductions.</summary>
    [JsonIgnore]
    public string QuantiteLabel => IsPositive ? $"+{Quantite}" : $"{Quantite}";

    /// <summary>Day/month part: "15/03" — returns "—" when date is unset.</summary>
    [JsonIgnore]
    public string DateDay => DateMouvement.Year > 1
        ? DateMouvement.ToString("dd/MM")
        : "—";

    /// <summary>Year part: "2025" — returns empty string when date is unset.</summary>
    [JsonIgnore]
    public string DateYear => DateMouvement.Year > 1
        ? DateMouvement.ToString("yyyy")
        : string.Empty;

    /// <summary>Full formatted date for tooltips/accessibility — "—" when unset.</summary>
    [JsonIgnore]
    public string DateLabel => DateMouvement.Year > 1
        ? DateMouvement.ToString("dd/MM/yyyy HH:mm")
        : "—";
}
