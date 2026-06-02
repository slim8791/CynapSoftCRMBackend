namespace CynapCRM.Services.InventoryAPI.Models.Dto
{
    public class StockSummaryDto
    {
        public int TotalProduits { get; set; }
        public int TotalQteDisponible { get; set; } 
        public int StocksVides { get; set; }    
        public int StocksFaibles { get; set; }     
        public int TotalDistributions { get; set; }   
        public int TotalQteDistribuee { get; set; }
        public DateTime? DernierMouvement { get; set; }
    }
}
