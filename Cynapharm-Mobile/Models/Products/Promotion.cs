namespace Cynapharm_Mobile.Models.Products;
public class Promotion
{
    public int Id { get; set; }
    public int? ProductId { get; set; }
    public string Titre { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? RemisePourcentage { get; set; }
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
}
