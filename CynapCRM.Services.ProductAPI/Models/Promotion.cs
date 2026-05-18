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
        public float? Pourcentage { get; set; }
        public DateTime? DateDebut { get; set; }
        [Required]
        public DateTime DateExpiration { get; set; }

        public bool EstActive { get; set; } = true;

        public virtual Lot? Lot { get; set; }
    }
}
