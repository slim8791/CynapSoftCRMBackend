namespace CynapCRM.Services.InventoryAPI.Models
{
    public class StockMovement
    {
        public int Id { get; set; }

        public int IdStock { get; set; }

        public int Quantity { get; set; }

        public string TypeMovement { get; set; } = string.Empty;

        public DateTime DateMovement { get; set; } = DateTime.UtcNow;
    }
}
