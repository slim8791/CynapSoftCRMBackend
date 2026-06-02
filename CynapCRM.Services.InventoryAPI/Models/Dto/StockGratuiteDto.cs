namespace CynapCRM.Services.InventoryAPI.Models.Dto
{
    public class StockGratuiteDto : StockDelegueDto
    {
        public int QteGratuite { get; set; }
        public string TypePromotion { get; set; } = string.Empty;
        public int QuantiteAchat { get; set; }
        public int QuantiteGratuite { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
    }
}
