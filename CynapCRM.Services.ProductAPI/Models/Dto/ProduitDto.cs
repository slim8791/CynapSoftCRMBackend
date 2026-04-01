namespace CynapCRM.Services.ProductAPI.Models.Dto
{
    public class ProduitDto
    {
        public int Id_Produit { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal PrixVente { get; set; }
        public decimal Prix_Creation { get; set; }
        public int TVA { get; set; }
        public IEnumerable<LotDto>? Lots { get; set; }
        public IEnumerable<SupportMarketingDto>? Supports { get; set; }
    }
}
