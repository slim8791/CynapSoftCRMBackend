using CynapCRM.Services.ProductAPI.Models.Dto;
using CynapCRM.Services.ProductAPI.Service.IService;

namespace CynapCRM.Services.ProductAPI.Service
{
    public class MarkettingService : IMarkettingService
    {
        public Task<FichierDto> AddFichierToSupportAsync(FichierDto fichierDto)
        {
            throw new NotImplementedException();
        }

        public Task<SupportMarketingDto> CreateUpdateSupportAsync(SupportMarketingDto supportDto)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteFichierAsync(int fichierId)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetActivePromotionsCountAsync(int productId)
        {
            throw new NotImplementedException();
        }

        public Task<ProductDashboardDto> GetProductDashboardAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<SupportMarketingDto>> GetSupportsByCampaignAsync(string campaignName)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<SupportMarketingDto>> GetSupportsByProductIdAsync(int productId)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetTotalLotsAsync(int productId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsSupportActiveAsync(int supportId)
        {
            throw new NotImplementedException();
        }
    }
}
