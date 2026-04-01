namespace CynapCRM.Services.InventoryAPI.Models
{
    public class Stock_Gratuite : Stock_Delegue
    {
        public int QteGratuite { get; set; }
        public string TypePromotion { get; set; } = string.Empty;
    }
}
