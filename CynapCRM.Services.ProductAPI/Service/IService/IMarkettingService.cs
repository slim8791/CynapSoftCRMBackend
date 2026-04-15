using CynapCRM.Services.ProductAPI.Models.Dto;

namespace CynapCRM.Services.ProductAPI.Service.IService
{
    public interface IMarkettingService
    {
        // ================================
        // 🔹 Gestion Marketing et Supports
        // ================================
        Task<IEnumerable<SupportMarketingDto>> GetSupportsByProductIdAsync(int productId);
        Task<SupportMarketingDto> CreateUpdateSupportAsync(SupportMarketingDto supportDto);
        Task<FichierDto> AddFichierToSupportAsync(FichierDto fichierDto);
        Task<bool> DeleteFichierAsync(int fichierId);

        // 🔥 Logique métier sur supports
        Task<bool> IsSupportActiveAsync(int supportId);
        Task<IEnumerable<SupportMarketingDto>> GetSupportsByCampaignAsync(string campaignName);

        Task<int> GetTotalLotsAsync(int productId);
        Task<int> GetActivePromotionsCountAsync(int productId);
        Task<ProductDashboardDto> GetProductDashboardAsync();
    }
}
