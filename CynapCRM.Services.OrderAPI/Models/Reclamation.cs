using CynapCRM.Services.OrderAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CynapCRM.Services.OrderAPI.Models
{
    public class Reclamation
    {
        [Key]
        public int Id_Rec { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;

        public DateTime DateReclamation { get; set; } = DateTime.Now;

        public string Statut { get; set; } = "Ouverte"; // Ouverte, En cours, Résolue

        // RELATION 
        [Required]
        public int Id_Commande { get; set; }

        [ForeignKey("Id_Commande")]
        public virtual Commande? Commande { get; set; }

        // client
        [Required]
        public int Id_Client { get; set; }
    }
}
 