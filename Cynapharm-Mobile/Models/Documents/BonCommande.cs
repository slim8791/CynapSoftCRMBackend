namespace Cynapharm_Mobile.Models.Documents;
public class BonCommande
{
    public int Id { get; set; }
    public string NumeroBon { get; set; } = string.Empty;
    public DateTime DateEmission { get; set; }
    public int CommandeId { get; set; }
    public decimal MontantTotal { get; set; }
    public string Statut { get; set; } = string.Empty;
}
