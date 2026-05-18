namespace Cynapharm_Mobile.Models.Inventory;
public class StockDelegue
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductNom { get; set; } = string.Empty;
    public int QuantiteAllouee { get; set; }
    public int QuantiteRestante { get; set; }
    public DateTime? DateExpiration { get; set; }
}
