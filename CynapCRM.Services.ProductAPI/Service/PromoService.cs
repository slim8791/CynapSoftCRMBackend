using CynapCRM.Services.ProductAPI.Models.Dto;
using CynapCRM.Services.ProductAPI.Service.IService;

namespace CynapCRM.Services.ProductAPI.Service
{
    public class PromoService : IPromoService
    {
        public Task<decimal> ApplyPromotionAsync(int productId, decimal prixInitial)
        {
            throw new NotImplementedException();
        }

        public Task<PromotionDto> CreateUpdatePromotionAsync(PromotionDto promotionDto)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeletePromotionAsync(int promotionId)
        {
            throw new NotImplementedException();
        }

        public Task<double> GetPromotionCoverageRateAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<PromotionDto>> GetPromotionsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<PromotionDto>> GetPromotionsByLotAsync(string numeroLot)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<PromotionDto>> GetPromotionsByProductAsync(int productId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsProductInPromotionAsync(int productId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsPromotionValidAsync(int promotionId)
        {
            throw new NotImplementedException();
        }
    }
}
