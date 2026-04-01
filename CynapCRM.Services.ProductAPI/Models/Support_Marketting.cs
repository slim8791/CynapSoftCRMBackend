using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CynapCRM.Services.ProductAPI.Models
{
    public class Support_Marketting
    {
        [Key]
        public int Id_SupportMarketting { get; set; }
        [Required]
        public string Type { get; set; } = string.Empty;

        [Required]
        public int Id_Produit { get; set; }

        [ForeignKey("Id_Produit")]
        public virtual Produit? Produit { get; set; }


        // Un support contient plusieurs fichiers
        public virtual ICollection<Fichier>? Fichiers { get; set; }
    }
}
