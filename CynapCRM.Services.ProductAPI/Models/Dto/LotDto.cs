namespace CynapCRM.Services.ProductAPI.Models.Dto
{
    public class LotDto
    {

        public string Numero { get; set; } = string.Empty;
        public DateTime DateExpiration { get; set; }
        public int Quantite { get; set; }

        public int Id_Produit { get; set; }

        // Logique métier (calculée)
        public bool IsExpired { get; set; }
        public bool IsOutOfStock { get; set; }

        public IEnumerable<PromotionDto>? Promotions { get; set; }

    }
}
