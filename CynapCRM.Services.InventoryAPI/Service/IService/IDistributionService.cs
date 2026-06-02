using CynapCRM.Services.InventoryAPI.Models;
using CynapCRM.Services.InventoryAPI.Models.Dto;

namespace CynapCRM.Services.InventoryAPI.Service.IService
{
    public interface IDistributionService
    {
        Task<bool> CreateOrUpdateEchantillonAsync(Echantillon echantillon);
        Task<EchantillonDto?> GetEchantillonByIdAsync(int idDistribution);
        Task<IEnumerable<EchantillonDto>> GetDistributionsByDelegueAsync(int idDelegue);
        Task<IEnumerable<EchantillonDto>> GetAllDistributionsAsync(
    int pageNumber, int pageSize);
        Task<IEnumerable<EchantillonDto>> GetDistributionsByMedecinAsync(int idMedecin);
        Task<IEnumerable<EchantillonDto>> GetDistributionsByPharmacienAsync(int idPharmacien);
        Task<bool> DeleteEchantillonAsync(int idDistribution);
    }
}
