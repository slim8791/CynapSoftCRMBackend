using CynapCRM.Services.FieldAPI.Models.Dto;

namespace CynapCRM.Services.FieldAPI.Service.IService
{
    public interface IRegionService
    {
        Task<IEnumerable<RegionDto>> GetAllRegionsAsync();
        Task<RegionDto?> CreateOrUpdateRegionAsync(RegionDto dto);

        Task<RegionDto?> GetRegionByIdAsync(int idRegion);

        Task<IEnumerable<RegionDto>> GetRegionsByDelegueAsync(int idDelegue);
        Task<IEnumerable<RegionDto>> GetRegionsBySuperviseurAsync(int idSuperviseur);

        Task<bool> DeleteRegionAsync(int idRegion);

        // Number of regions covered by a superviseur (or delegue id — kept for compatibility)
        Task<int> GetNombreRegionsCouvreAsync(int idDelegue);

    }
}
