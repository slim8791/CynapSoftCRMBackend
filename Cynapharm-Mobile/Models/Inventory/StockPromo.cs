namespace Cynapharm_Mobile.Models.Inventory;
public class StockPromo
{
    public int Id { get; set; }
    public int PromotionId { get; set; }
    public string PromotionTitre { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductNom { get; set; } = string.Empty;
    public int Quantite { get; set; }
}
