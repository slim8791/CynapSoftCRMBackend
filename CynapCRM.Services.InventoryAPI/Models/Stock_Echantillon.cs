namespace CynapCRM.Services.InventoryAPI.Models
{
    public class Stock_Echantillon : Stock_Delegue
    {
        public int       QteEchantillon { get; set; }
        public string?   Description    { get; set; }
        public DateTime? DateDebut      { get; set; }
        public DateTime? DateFin        { get; set; }
    }
}
