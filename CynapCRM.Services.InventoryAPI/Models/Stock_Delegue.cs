using System.ComponentModel.DataAnnotations;

namespace CynapCRM.Services.InventoryAPI.Models
{
    public class Stock_Delegue
    {
        [Key]
        public int Id_stock { get; set; }

        [Required]
        public int QteDisponible { get; set; }

        // Références vers d'autres services 
        public int Id_User_Delegue { get; set; }
        public int Id_Produit { get; set; }
        public string NumeroLot { get; set; } = string.Empty;
    }
}
