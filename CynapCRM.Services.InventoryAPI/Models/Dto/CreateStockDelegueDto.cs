namespace CynapCRM.Services.InventoryAPI.Models.Dto
{
    public class CreateStockDelegueDto
    {

        public int Id_User_Delegue { get; set; }
        public int Id_Produit { get; set; }
        public string NumeroLot { get; set; }
        public int Quantite { get; set; }

    }
}
