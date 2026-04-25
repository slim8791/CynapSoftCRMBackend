using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CynapCRM.Services.ProductAPI.Models
{
    public class Produit
    {
        [Key]
        public int Id_Produit { get; set; }
        [Required]
        public string Nom { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrixVente { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Prix_Creation { get; set; }
        public int TVA { get; set; }
        public bool IsArchived { get; set; } = false;
        public bool IsActive { get; set; } = true;
        // Un produit peut avoir plusieurs lots 
        public virtual ICollection<Lot>? Lots { get; set; }

        // un produit pet avoir plusieur supportMarketting
        public virtual ICollection<Support_Marketting>? Supports { get; set; }
    }
}
