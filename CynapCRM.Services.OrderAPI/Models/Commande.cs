using CynapCRM.Services.OrderAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CynapCRM.Services.OrderAPI.Models
{
    public class Commande
    {
        [Key]
        public int Id_Commande { get; set; }

        [Required]
        public DateTime DateCommande { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontantTotalHT { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontantTTC { get; set; }

        [Required]
        public EtatCommande Statut { get; set; } = EtatCommande.Brouillon;

        // relation essentielle avec le client


        [Required]
        public int Id_Client { get; set; } // Le client (Pharmacie/Grossiste) ===(AuthAPI)

        // Une commande a plusieurs lignes
        public virtual ICollection<LigneCommande> Lignes { get; set; } = new List<LigneCommande>();

        // Une commande peut avoir des réclamations
        public virtual ICollection<Reclamation>? Reclamations { get; set; }
    }
}