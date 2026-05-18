namespace Cynapharm_Mobile.Models.Inventory;

public class StockDisplayItem
{
    public int ProductId { get; set; }
    public string ProductNom { get; set; } = string.Empty;
    public string QuantiteLabel { get; set; } = string.Empty;
    public string? ExpiryLabel { get; set; }

    // Raw remaining quantity — used by quota enforcement
    public int QuantiteRestante { get; set; }

    // True for échantillon rows; false for promo stock rows
    public bool IsEchantillon { get; set; }

    public bool HasExpiry => ExpiryLabel != null;

    // Distribute button is only active for samples that still have stock
    public bool CanDistribute => IsEchantillon && QuantiteRestante > 0;
}
