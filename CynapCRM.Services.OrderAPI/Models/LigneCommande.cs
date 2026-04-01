using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CynapCRM.Services.OrderAPI.Models
{
    public class LigneCommande
    {
        [Key]
        public int Id_Ligne { get; set; }

        [Required]
        public int Quantite { get; set; }
        [Column(TypeName = "decimal(5,2)")]

        public decimal Remise { get; set; } 


        [Required]
        public int Id_Commande { get; set; } 

        [ForeignKey("Id_Commande")]
        public virtual Commande? Commande { get; set; }

        [Required]
        public int Id_Produit { get; set; } // Lien vers ProductAPI (pour savoir quoi on achète)

        [Required]
        public string NumeroLot { get; set; } = string.Empty; // Lien vers InventoryAPI (pour la traçabilité)
    }
}