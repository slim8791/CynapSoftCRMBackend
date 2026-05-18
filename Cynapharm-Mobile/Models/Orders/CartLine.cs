namespace Cynapharm_Mobile.Models.Orders;

public class CartLine
{
    public int ProductId { get; set; }
    public string ProductNom { get; set; } = string.Empty;
    public int Quantite { get; set; }

    // PrixOriginal = base price before any discount
    public decimal PrixOriginal { get; set; }

    // PrixUnitaire = effective price sent to the server (may equal PrixOriginal when no promo)
    public decimal PrixUnitaire { get; set; }

    // 0 means no active promotion
    public decimal RemisePourcentage { get; set; }
    public string? PromoTitre { get; set; }

    public bool HasPromo => RemisePourcentage > 0;
    public decimal SousTotal => Quantite * PrixUnitaire;
    public decimal EconomieTotale => Quantite * (PrixOriginal - PrixUnitaire);
}
