namespace CynapCRM.Services.InventoryAPI.Models
{
    public class Stock_Gratuite : Stock_Delegue
    {
        public int QteGratuite { get; set; }
        public string TypePromotion { get; set; } = string.Empty;
        public int QuantiteAchat { get; set; }
        public int QuantiteGratuite { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
    }
}
