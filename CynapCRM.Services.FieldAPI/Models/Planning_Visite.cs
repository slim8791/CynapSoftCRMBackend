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

        // ✅ Un planning contient plusieurs visites
        public virtual ICollection<Visite> Visites { get; set; } = new List<Visite>();
    }
}