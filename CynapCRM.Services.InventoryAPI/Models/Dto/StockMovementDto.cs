namespace CynapCRM.Services.InventoryAPI.Models.Dto
{
    public class StockMovementDto
    {

        public int Id_Movement { get; set; }
        public int Id_Stock { get; set; }
        public int Quantite { get; set; }
        public string TypeMovement { get; set; } = string.Empty;
        public DateTime DateMovement { get; set; }
        public string? Description { get; set; }

        public int Id_User_Delegue { get; set; }
        public int Id_Produit { get; set; }

    }
}
