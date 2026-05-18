namespace Cynapharm_Mobile.Models.Products;
public class Lot
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string NumeroLot { get; set; } = string.Empty;
    public DateTime DateFabrication { get; set; }
    public DateTime DateExpiration { get; set; }
    public int QuantiteDisponible { get; set; }
}
