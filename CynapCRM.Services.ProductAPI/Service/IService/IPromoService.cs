using CynapCRM.Services.ProductAPI.Models.Dto;

namespace CynapCRM.Services.ProductAPI.Service.IService
{
    public interface IPromoService
    {
        // ================================
        // 🔹 Gestion des Promotions
        // ================================
        Task<IEnumerable<PromotionDto>> GetPromotionsAsync();
        Task<PromotionDto> CreateUpdatePromotionAsync(PromotionDto promotionDto);
        Task<bool> DeletePromotionAsync(int promotionId);

        // 🔥 Logique métier sur promotions
        Task<decimal> ApplyPromotionAsync(int productId, decimal prixInitial);
        Task<bool> IsProductInPromotionAsync(int productId); // ✅ renommé pour cohérence
        Task<IEnumerable<PromotionDto>> GetPromotionsByProductAsync(int productId);
        Task<IEnumerable<PromotionDto>> GetPromotionsByLotAsync(string numeroLot);
        Task<bool> IsPromotionValidAsync(int promotionId);
        Task<double> GetPromotionCoverageRateAsync();
    }
}
