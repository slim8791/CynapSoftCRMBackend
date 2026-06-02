using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CynapCRM.Services.OrderAPI.Models
{
    public class Commande
    {
        [Key]
        public int Id_Commande { get; set; }

        [Required]
        public DateTime DateCommande { get; set; } = DateTime.UtcNow; // FIX: UtcNow

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontantTotalHT { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontantTTC { get; set; }

        [Required]
        public EtatCommande Statut { get; set; } = EtatCommande.Brouillon;

        [Required]
        public int Id_Client { get; set; }

        // FIX: soft delete
        public bool IsDeleted { get; set; } = false;

        // FIX: motif annulation
        public string? MotifAnnulation { get; set; }

        public virtual ICollection<LigneCommande> Lignes { get; set; } = new List<LigneCommande>();

        public virtual ICollection<Reclamation>? Reclamations { get; set; }
    }
}