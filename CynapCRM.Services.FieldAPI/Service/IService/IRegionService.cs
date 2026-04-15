using CynapCRM.Services.FieldAPI.Models.Dto;

namespace CynapCRM.Services.FieldAPI.Service.IService
{
    public interface IRegionService
    {
        // ================================
        // 🔹 RÉGION
        // ================================

        Task<RegionDto?> CreateOrUpdateRegionAsync(RegionDto dto);
        Task<IEnumerable<RegionDto>> GetRegionsByDelegueAsync(int idDelegue);
        Task<bool> DeleteRegionAsync(int idRegion);

        // Logique métier
        Task<bool> AssignRegionToDelegueAsync(int idRegion, int idDelegue);
        // 🔥 NOUVEAU : nombre de régions couvertes
        // KPI terrain important
        Task<int> GetNombreRegionsCouvreAsync(int idDelegue);
    }
}
