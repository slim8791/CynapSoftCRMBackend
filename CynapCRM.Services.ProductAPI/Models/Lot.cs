using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CynapCRM.Services.ProductAPI.Models
{
    public class Lot
    {
        [Key]
        public string Numero { get; set; } = string.Empty; 

        [Required]
        public DateTime DateExpiration { get; set; }

        [Required]
        public int Quantite { get; set; }

        // Clé étrangère vers Produit
        public int Id_Produit { get; set; }
        [ForeignKey("Id_Produit")]
        public virtual Produit? Produit { get; set; }

        // Lien avec la promotion 
        public int? Id_Promo { get; set; }
        [ForeignKey("Id_Promo")]
        public virtual ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();
    }
}
