namespace CynapCRM.Services.FieldAPI.Models.Dto
{
    public class VisiteDto
    {

        public int IdVisite { get; set; }
        public DateTime DateVisite { get; set; }
        public VisiteType Type { get; set; }
        public int Id_User_Delegue { get; set; }
        public int? IdMedecin { get; set; }
        public int? IdPharmacien { get; set; }

        public int? IdPlanning { get; set; }

        public bool IsCompleted { get; set; }
        public bool HasRapport { get; set; }
        public string? ClientNom { get; set; }

    }
}
