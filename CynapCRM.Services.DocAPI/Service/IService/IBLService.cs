using CynapCRM.Services.DocAPI.Models.Dto;

namespace CynapCRM.Services.DocAPI.Service.IService
{
    public interface IBLService
    {
        Task<IEnumerable<BonLivraisonDto>> GetBonsLivraisonByCommandeAsync(
            int idCommande);
        Task<IEnumerable<BonLivraisonDto>> GetAllBonsLivraisonAsync(int pageNumber, int pageSize);

        Task<BonLivraisonDto?> CreateOrUpdateBonLivraisonAsync(BonLivraisonDto blDto);
        Task<BonLivraisonDto?> GetBonLivraisonByIdAsync(int idBL);
        Task<IEnumerable<BonLivraisonDto>> GetBonsLivraisonByClientAsync(int idClient);
        Task<bool> DeleteBonLivraisonAsync(int idBL);
        Task<IEnumerable<BonLivraisonDto>> GetBonsLivraisonByDateAsync(DateTime startDate, DateTime endDate);
    }
}
