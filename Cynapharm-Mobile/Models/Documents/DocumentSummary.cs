namespace Cynapharm_Mobile.Models.Documents;
public class DocumentSummary
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Statut { get; set; } = string.Empty;
    public decimal? Montant { get; set; }
}
