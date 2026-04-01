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
        // listes des lots associés au produit
        public IEnumerable<LotDto>? Lots { get; set; }
        // listes des supports associés au produit
        public IEnumerable<SupportMarketingDto>? Supports { get; set; }
    }
}
