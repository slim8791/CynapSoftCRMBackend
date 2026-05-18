namespace Cynapharm_Mobile.Models.Documents;
public class BonLivraison
{
    public int Id { get; set; }
    public string NumeroBon { get; set; } = string.Empty;
    public DateTime DateLivraison { get; set; }
    public int CommandeId { get; set; }
    public string Statut { get; set; } = string.Empty;
}
