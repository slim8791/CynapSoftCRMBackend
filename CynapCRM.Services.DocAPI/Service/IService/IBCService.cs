using CynapCRM.Services.DocAPI.Models.Dto;

namespace CynapCRM.Services.DocAPI.Service.IService
{
    public interface IBCService
    {

        Task<bool> DeleteBonCommandeAsync(int idBC);
        Task<IEnumerable<BonCommandeDto>> GetAllBonsCommandeAsync(int pageNumber, int pageSize);
        Task<IEnumerable<BonCommandeDto>> GetBonsCommandeByCommandeAsync(
            int idCommande);
        Task<BonCommandeDto?> CreateOrUpdateBonCommandeAsync(BonCommandeDto bcDto);
        Task<BonCommandeDto?> GetBonCommandeByIdAsync(int idBC);
        Task<IEnumerable<BonCommandeDto>> GetBonsCommandeByClientAsync(int idClient);
        Task<IEnumerable<BonCommandeDto>> GetBonsCommandeByDateAsync(DateTime startDate, DateTime endDate);
    }
}
