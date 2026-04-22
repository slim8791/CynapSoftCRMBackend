namespace CynapCRM.Services.FieldAPI.Models.Dto
{
    public class ActiviteHistoriqueDto
    {
        public int Id_Visite { get; set; }
        public DateTime Date { get; set; }

        public VisiteType Type { get; set; }
        public bool HasRapport { get; set; }
    }
}
