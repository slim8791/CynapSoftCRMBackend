namespace CynapCRM.Services.InventoryAPI.Models.Dto
{
    public class StockEchantillonDto : StockDelegueDto
    {
        public int       QteEchantillon { get; set; }
        public string?   Description    { get; set; }
        public DateTime? DateDebut      { get; set; }
        public DateTime? DateFin        { get; set; }
    }
}
