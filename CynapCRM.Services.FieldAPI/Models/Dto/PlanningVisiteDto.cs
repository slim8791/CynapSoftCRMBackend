namespace CynapCRM.Services.FieldAPI.Models.Dto
{
    public class PlanningVisiteDto
    {
        public int Id_Planning { get; set; }

        public DateTime Date { get; set; }

        public TimeSpan HeureDebut { get; set; }
        public TimeSpan HeureFin { get; set; }


        public EtatPlanning Etat { get; set; }

        public int Id_User_Delegue { get; set; }
    }
}
