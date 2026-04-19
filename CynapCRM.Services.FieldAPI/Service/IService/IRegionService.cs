using CynapCRM.Services.FieldAPI.Models.Dto;

namespace CynapCRM.Services.FieldAPI.Service.IService
{
    public interface IRegionService
    {

        // ================================
        // 🔹 RÉGIONS
        // ================================

        Task<RegionDto?> CreateOrUpdateRegionAsync(RegionDto dto);

        Task<RegionDto?> GetRegionByIdAsync(int idRegion);

        Task<IEnumerable<RegionDto>> GetRegionsByDelegueAsync(int idDelegue);

        Task<bool> DeleteRegionAsync(int idRegion);

        // ================================
        // 🔹 LOGIQUE MÉTIER / KPI
        // ================================

        // Nombre de régions couvertes par un délégué
        Task<int> GetNombreRegionsCouvreAsync(int idDelegue);

    }
}
