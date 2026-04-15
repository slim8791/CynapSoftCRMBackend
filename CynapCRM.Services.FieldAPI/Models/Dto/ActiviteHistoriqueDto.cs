namespace CynapCRM.Services.FieldAPI.Models.Dto
{
    public class ActiviteHistoriqueDto
    {
        public int Id_Visite { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public string NomTournee { get; set; }
        public bool HasRapport { get; set; }
    }
}
