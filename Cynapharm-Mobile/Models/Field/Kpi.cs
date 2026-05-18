namespace Cynapharm_Mobile.Models.Field;
public class Kpi
{
    public int Id { get; set; }
    public int DelegueId { get; set; }
    public string Periode { get; set; } = string.Empty;
    public string Indicateur { get; set; } = string.Empty;
    public decimal Valeur { get; set; }
    public DateTime DateCalcul { get; set; }
}
