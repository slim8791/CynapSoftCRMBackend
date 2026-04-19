using System.ComponentModel.DataAnnotations;

namespace CynapCRM.Services.InventoryAPI.Models
{
    public class StockMovement
    {

        [Key]
        public int Id_Movement { get; set; }

        // Stock concerné
        [Required]
        public int Id_Stock { get; set; }

        // Quantité déplacée
        [Required]
        public int Quantite { get; set; }

        /// <summary>
        /// Type du mouvement :
        /// IN, OUT, TRANSFER_IN, TRANSFER_OUT
        /// </summary>
        [Required]
        public string TypeMovement { get; set; } = string.Empty;

        // Date du mouvement
        public DateTime DateMovement { get; set; } = DateTime.UtcNow;

        // Description libre (optionnelle)
        public string? Description { get; set; }

    }
}
