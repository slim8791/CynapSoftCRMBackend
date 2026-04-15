namespace CynapCRM.Services.InventoryAPI.Models.Dto
{
    public class StockMovementDto
    {
        public int IdStock { get; set; }

        public int Quantity { get; set; }

        public string TypeMovement { get; set; }

        public DateTime DateMovement { get; set; }
    }
}
