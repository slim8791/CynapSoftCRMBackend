namespace CynapCRM.Services.FieldAPI.Models.Dto
{
    public class PerformanceDto
    {
        public string TypeObjectif { get; set; } = string.Empty;
        public int ValeurCible { get; set; }
        public int ValeurRealisee { get; set; }
        public double Pourcentage { get; set; }
    }
}
