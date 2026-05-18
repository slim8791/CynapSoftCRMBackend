namespace Cynapharm_Mobile.Models.Inventory;
public class StockMouvement
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductNom { get; set; } = string.Empty;
    public int Quantite { get; set; }
    public string TypeMouvement { get; set; } = string.Empty;
    public DateTime DateMouvement { get; set; }
}
