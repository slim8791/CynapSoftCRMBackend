using CynapCRM.Services.FieldAPI.Models.Dto;

namespace CynapCRM.Services.FieldAPI.Service.IService
{
    public interface IObjectifService
    {
        // ================================
        // 🔹 OBJECTIFS
        // ================================

        Task<ObjectifDelegueDto?> CreateOrUpdateObjectifAsync(ObjectifDelegueDto dto);
        Task<ObjectifDelegueDto?> GetObjectifByDelegueAsync(int idDelegue);
        Task<bool> DeleteObjectifAsync(int idObjectif);

        // Logique métier
        Task<bool> UpdateObjectifValueAsync(int idObjectif, int nouvelleValeur);
        Task<IEnumerable<PerformanceDto>> CalculatePerformanceAsync(int idDelegue);
        // 🔥 NOUVEAU : KPI global simplifié
        Task<double> GetPerformanceRateAsync(int idDelegue);
    }
}
