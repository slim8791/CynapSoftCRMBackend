using System.ComponentModel.DataAnnotations;

namespace CynapCRM.Services.InventoryAPI.Models
{
    public class StockMovement
    {

        [Key]
        public int Id_Movement { get; set; }

        [Required]
        public int Id_Stock { get; set; }

        [Required]
        public int Quantite { get; set; }


        [Required]
        public string TypeMovement { get; set; } = string.Empty;

        public DateTime DateMovement { get; set; } = DateTime.UtcNow;

        public string? Description { get; set; }

    }
}
