namespace Cynapharm_Mobile.Models.Documents;
public class Facture
{
    public int Id { get; set; }
    public string NumeroFacture { get; set; } = string.Empty;
    public DateTime DateFacture { get; set; }
    public int CommandeId { get; set; }
    public decimal MontantHT { get; set; }
    public decimal TVA { get; set; }
    public decimal MontantTTC { get; set; }
    public string Statut { get; set; } = string.Empty;
}
