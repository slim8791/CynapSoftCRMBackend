namespace CynapCRM.Services.FieldAPI.Models.Dto
{
    public class PlanningVisiteDto
    {
        public int Id_Planning { get; set; }

        public DateTime Date { get; set; }

        public TimeSpan HeureDebut { get; set; }
        public TimeSpan HeureFin  { get; set; }

        public EtatPlanning Etat { get; set; }

        public int Id_User_Delegue { get; set; }

        // Contact cible
        public int? Id_Medecin    { get; set; }
        public int? Id_Pharmacien { get; set; }

        // Type de visite prévue (1=Médecin 2=Pharmacien)
        public int TypeVisite { get; set; } = 1;
    }
}
