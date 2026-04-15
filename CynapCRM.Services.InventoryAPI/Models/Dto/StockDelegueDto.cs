namespace CynapCRM.Services.InventoryAPI.Models.Dto
{
    public class StockDelegueDto
    {
        public int Id_stock { get; set; }

        public int Id_User_Delegue { get; set; }

        public int Id_Produit { get; set; }

        public string NumeroLot { get; set; }

        public DateTime DateExpiration { get; set; }

        public int QteDisponible { get; set; }

        public int QteReservee { get; set; }
    }
}
