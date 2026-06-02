using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Inventory;

public class StockDisplayItem
{
    // Backend stock identifier — required to build the correct EchantillonDto on distribution
    public int StockId { get; set; }
    public string NumeroLot { get; set; } = string.Empty;

    public int ProductId { get; set; }
    public string ProductNom { get; set; } = string.Empty;
    public string QuantiteLabel { get; set; } = string.Empty;
    public string? ExpiryLabel { get; set; }
    public string? PromoDetails { get; set; }

    // Raw remaining quantity — used by quota enforcement
    public int QuantiteRestante { get; set; }

    // Original allocated quantity — used to compute ProgressValue
    public int QuantiteAllouee { get; set; }

    // True for échantillon rows; false for promo stock rows
    public bool IsEchantillon { get; set; }

    [JsonIgnore]
    public bool HasExpiry => ExpiryLabel != null;

    [JsonIgnore]
    public bool HasPromoDetails => !string.IsNullOrEmpty(PromoDetails);

    // Distribute button is only active for samples that still have stock
    [JsonIgnore]
    public bool CanDistribute => IsEchantillon && QuantiteRestante > 0;

    /// <summary>
    /// Fraction of allocated stock remaining (0–1). Used by ProgressBar.
    /// Falls back to 1 if QuantiteAllouee is unknown but remaining > 0, 0 otherwise.
    /// </summary>
    [JsonIgnore]
    public float ProgressValue =>
        QuantiteAllouee > 0
            ? Math.Clamp((float)QuantiteRestante / QuantiteAllouee, 0f, 1f)
            : 0f; // no allocated quantity known → show empty bar (covers promo cards)

    /// <summary>
    /// True only for échantillon cards that have run out of stock.
    /// Promo cards never show the red warning border, regardless of quantity.
    /// </summary>
    [JsonIgnore]
    public bool ShowLowStockWarning => IsEchantillon && QuantiteRestante <= 0;
}
