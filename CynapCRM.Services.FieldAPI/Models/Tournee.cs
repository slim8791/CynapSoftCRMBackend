using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CynapCRM.Services.FieldAPI.Models
{
    public class Tournee
    {
        [Key]
        public int Id_Tournee { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string Statut { get; set; } = "En attente";
        [MaxLength(200)]
        public string Nom { get; set; }

        // Une tournée appartient à un planning de délégué
        [Required]
        public int Id_Planning { get; set; }

        [ForeignKey("Id_Planning")]
        public virtual PlanningVisite? Planning { get; set; }

        // Une tournée contient plusieurs visites 
        public virtual ICollection<Visite> Visites { get; set; } = new List<Visite>();

        // ID du délégué responsable
        [Required]
        public int Id_User_Delegue { get; set; }
    }
}