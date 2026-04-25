namespace CynapCRM.Services.FieldAPI.Models.Dto
{
    public class PerformanceDto
    {
        public TypeObjectif Type { get; set; }
        public int ValeurCible { get; set; }
        public int ValeurRealisee { get; set; }
        public double Pourcentage { get; set; }
    }
}
