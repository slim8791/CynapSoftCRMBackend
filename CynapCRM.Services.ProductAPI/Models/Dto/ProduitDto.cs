namespace CynapCRM.Services.ProductAPI.Models.Dto
{
    public class ProduitDto
    {
        public int Id_Produit { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Prix_Vente { get; set; }
        public decimal Prix_Creation { get; set; }
        public int TVA { get; set; }
        // 🔹 Lots associés au produit
        public IEnumerable<LotDto>? Lots { get; set; }

        // 🔹 Supports marketing liés au produit
        public IEnumerable<SupportMarketingDto>? Supports { get; set; }

        // 🔹 État métier
        public bool IsActive { get; set; }
        public bool IsArchived { get; set; }
    }
}
