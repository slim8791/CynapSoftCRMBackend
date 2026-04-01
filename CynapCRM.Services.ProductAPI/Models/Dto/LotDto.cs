namespace CynapCRM.Services.ProductAPI.Models.Dto
{
    public class LotDto
    {
        public string Numero { get; set; } = string.Empty;
        public DateTime DateExpiration { get; set; }
        public int Quantite { get; set; }

        // Clé étrangère vers Produit
        public int Id_Produit { get; set; }

        // Lien avec la promotion
        public IEnumerable<PromotionDto>? Promotions { get; set; }
    }
}
