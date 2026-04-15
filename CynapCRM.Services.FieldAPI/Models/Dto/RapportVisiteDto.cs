namespace CynapCRM.Services.FieldAPI.Models.Dto
{
    public class RapportVisiteDto
    {
        public int Id_Rapport { get; set; }

        public string Commentaire { get; set; } = string.Empty;

        public string Resultat { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public int Id_Visite { get; set; }

        public int Id_User_Delegue { get; set; }
    }
}
