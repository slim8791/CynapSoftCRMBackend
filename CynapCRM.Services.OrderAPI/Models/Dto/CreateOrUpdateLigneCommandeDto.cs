using System.ComponentModel.DataAnnotations;

namespace CynapCRM.Services.OrderAPI.Models.Dto
{
    public class CreateOrUpdateLigneCommandeDto
    {
        public int Id_Ligne { get; set; }

        [Required]
        public int Id_Commande { get; set; }

        [Required]
        public int Id_Produit { get; set; }

        // FIX: validation quantité
        [Range(1, 9999, ErrorMessage = "La quantité doit être entre 1 et 9999.")]
        public int Quantite { get; set; }

        // FIX: validation remise
        [Range(0, 100, ErrorMessage = "La remise doit être entre 0 et 100.")]
        public decimal Remise { get; set; }

        // FIX: validation prix
        [Range(0.01, double.MaxValue, ErrorMessage = "Le prix unitaire doit être positif.")]
        public decimal PrixUnitaire { get; set; }
    }
}