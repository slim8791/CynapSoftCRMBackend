using CynapCRM.Services.InventoryAPI.Models.Dto;

namespace CynapCRM.Services.InventoryAPI.Service.IService
{
    public interface IDistributionService
    {
        Task<EchantillonDto?> CreateOrUpdateEchantillonAsync(EchantillonDto echantillonDto);
        Task<EchantillonDto?> GetEchantillonByIdAsync(int idDistribution);
        Task<IEnumerable<EchantillonDto>> GetDistributionsByMedecinAsync(int idMedecin);
        Task<IEnumerable<EchantillonDto>> GetDistributionsByPharmacienAsync(int idPharmacien);
        Task<bool> DeleteEchantillonAsync(int idDistribution);
    }
}
