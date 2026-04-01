using System.ComponentModel.DataAnnotations;

namespace CynapCRM.Services.FieldAPI.Models
{
    public class PlanningVisite
    {
        [Key]
        public int Id_Planning { get; set; }
        public DateTime Date { get; set; }
        public DateTime HeureDebut { get; set; }
        public DateTime HeureFin { get; set; }
        public string Etat { get; set; } = "En attente"; 

        public int Id_User_Delegue { get; set; } 
        public virtual ICollection<Tournee>? Tournees { get; set; } = new List<Tournee>();
    }
}
