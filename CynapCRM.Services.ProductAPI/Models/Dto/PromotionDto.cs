namespace CynapCRM.Services.ProductAPI.Models.Dto
{
    public class PromotionDto
    {
        public int Id_Promo { get; set; }

        public string CodePromo { get; set; } = string.Empty;

        public TypePromotion TypePromotion { get; set; } = TypePromotion.Pourcentage;

        // Percentage promotion
        public float? Pourcentage { get; set; }

        // Freebie promotion
        public int? SeuilAchat { get; set; }
        public int? QuantiteGratuite { get; set; }

        // Scope
        public bool PorteeSurTousLesLots { get; set; } = false;
        public string? NumeroLot { get; set; }
        public int? Id_Produit { get; set; }

        // Common
        public DateTime DateDebut { get; set; }
        public DateTime DateExpiration { get; set; }
        public bool EstActive { get; set; }

        // Calculated
        public bool IsValid { get; set; }
    }
}
