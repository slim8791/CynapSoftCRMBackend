using CynapCRM.Services.OrderAPI.Models;
using CynapCRM.Services.OrderAPI.Models.Dto;
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


        public StatutReclamation Statut { get; set; } = StatutReclamation.Ouverte;

        // RELATION 
        [Required]
        public int Id_Commande { get; set; }

        [ForeignKey("Id_Commande")]
        public virtual Commande? Commande { get; set; }
        [Required]
        public int Id_Ligne { get; set; }

        [ForeignKey("Id_Ligne")]
        public virtual LigneCommande? LigneCommande { get; set; }

        // client
        [Required]
        public int Id_Client { get; set; }
    }
}
 