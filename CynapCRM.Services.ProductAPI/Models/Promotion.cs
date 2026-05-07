using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CynapCRM.Services.ProductAPI.Models
{
    public class Promotion
    {
        [Key]
        public int Id_Promo { get; set; }

        [Required]
        [StringLength(50)]
        public string CodePromo { get; set; } = string.Empty;

        public TypePromotion TypePromotion { get; set; } = TypePromotion.Pourcentage;

        // ── Percentage promotion ──────────────────────────────
        // Nullable: only required when TypePromotion == Pourcentage
        [Range(0, 100)]
        public float? Pourcentage { get; set; }

        // ── Freebie promotion (buy X get Y free) ─────────────
        // SeuilAchat: minimum units the customer must buy (e.g. 5)
        public int? SeuilAchat { get; set; }

        // QuantiteGratuite: free units given (e.g. 1)
        public int? QuantiteGratuite { get; set; }

        // ── Scope ─────────────────────────────────────────────
        // PorteeSurTousLesLots = true → applies to every lot of Id_Produit
        // PorteeSurTousLesLots = false → applies only to NumeroLot
        public bool PorteeSurTousLesLots { get; set; } = false;

        // FK to a specific lot (nullable when PorteeSurTousLesLots = true)
        public string? NumeroLot { get; set; }

        // Product ID used when PorteeSurTousLesLots = true
        public int? Id_Produit { get; set; }

        // ── Common ────────────────────────────────────────────
        public DateTime DateDebut { get; set; }

        [Required]
        public DateTime DateExpiration { get; set; }

        public bool EstActive { get; set; } = true;

        public virtual Lot? Lot { get; set; }
    }
}
