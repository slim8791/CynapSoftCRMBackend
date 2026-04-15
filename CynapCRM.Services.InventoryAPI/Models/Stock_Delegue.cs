using System.ComponentModel.DataAnnotations;

namespace CynapCRM.Services.InventoryAPI.Models
{
    public class Stock_Delegue
    {
        public int Id_stock { get; set; }

        public int Id_User_Delegue { get; set; }

        public int Id_Produit { get; set; }

        public string NumeroLot { get; set; } = string.Empty;

        public DateTime DateExpiration { get; set; }

        public int QteDisponible { get; set; }

        public int QteReservee { get; set; } = 0;
    }
}
