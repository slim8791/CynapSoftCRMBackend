namespace CynapCRM.Services.InventoryAPI.Models.Dto
{
    public class StockGratuiteDto : StockDelegueDto
    {
        public int QteGratuite { get; set; }
        public string TypePromotion { get; set; } = string.Empty;
    }
}
