namespace CynapCRM.Services.FieldAPI.Models.Dto
{
    public class VisiteDto
    {
        public int IdVisite { get; set; }
        public DateTime Date { get; set; }
        public VisiteType Type { get; set; }   // enum ici
        public int IdDelegue { get; set; }
        public int? IdMedecin { get; set; }
        public int? IdPharmacien { get; set; }
        public int? IdTournee { get; set; }
        public string? ClientNom { get; set; }
        public bool RapportExiste { get; set; }
    }
}
