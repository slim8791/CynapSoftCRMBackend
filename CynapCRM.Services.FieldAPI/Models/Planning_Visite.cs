using System.ComponentModel.DataAnnotations;

namespace CynapCRM.Services.FieldAPI.Models
{
    public class Planning_Visite
    {
        [Key]
        public int Id_Planning { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public TimeSpan HeureDebut { get; set; }

        [Required]
        public TimeSpan HeureFin { get; set; }

        [Required]
        public EtatPlanning Etat { get; set; } = EtatPlanning.EnAttente;

        // Délégué concerné
        [Required]
        public int Id_User_Delegue { get; set; }

        // Contact cible (médecin ou pharmacien/grossiste)
        public int? Id_Medecin    { get; set; }
        public int? Id_Pharmacien { get; set; }

        // Type de visite prévue : 1=Médecin  2=Pharmacien  (VisiteType enum)
        public int TypeVisite { get; set; } = 1;

        //  Un planning contient plusieurs visites
        public virtual ICollection<Visite> Visites { get; set; } = new List<Visite>();
    }
}