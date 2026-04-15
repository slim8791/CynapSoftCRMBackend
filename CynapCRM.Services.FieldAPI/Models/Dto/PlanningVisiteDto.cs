namespace CynapCRM.Services.FieldAPI.Models.Dto
{
    public class PlanningVisiteDto
    {
        public int Id_Planning { get; set; }

        public DateTime Date { get; set; }

        public DateTime HeureDebut { get; set; }

        public DateTime HeureFin { get; set; }

        public string Etat { get; set; } = string.Empty;

        public int Id_User_Delegue { get; set; }
    }
}
