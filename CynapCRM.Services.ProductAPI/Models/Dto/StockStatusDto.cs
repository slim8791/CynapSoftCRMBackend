namespace CynapCRM.Services.ProductAPI.Models.Dto
{
    public class StockStatusDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int TotalStock { get; set; }
        public bool IsAvailable { get; set; }
    }
}
