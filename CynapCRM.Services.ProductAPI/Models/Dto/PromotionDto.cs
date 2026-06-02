namespace CynapCRM.Services.ProductAPI.Models.Dto
{
    public class PromotionDto
    {
        public int Id_Promo { get; set; }

        public string CodePromo { get; set; } = string.Empty;
        public TypePromotion TypePromotion { get; set; }

        public float? Pourcentage { get; set; }

        public DateTime? DateDebut { get; set; }
        public DateTime DateExpiration { get; set; }
        public bool EstActive { get; set; }

        // Calculated
        public bool IsValid { get; set; }
        public string? NumeroLot { get; set; } = string.Empty;
    }
}
