namespace CynapCRM.Services.FieldAPI.Models.Dto
{
    public class CreateVisiteDto
    {

        public DateTime DateVisite { get; set; }
        public VisiteType Type { get; set; }

        public int IdDelegue { get; set; }
        public int? IdMedecin { get; set; }
        public int? IdPharmacien { get; set; }

        public int? IdPlanning { get; set; }

    }
}
