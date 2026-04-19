using CynapCRM.Services.FieldAPI.Models.Dto;

namespace CynapCRM.Services.FieldAPI.Service.IService
{
    public interface IPlanningService
    {

        // ================================
        // 📅 PLANNING DE VISITES
        // ================================

        Task<PlanningVisiteDto?> CreateOrUpdatePlanningAsync(PlanningVisiteDto dto);

        Task<PlanningVisiteDto?> GetPlanningByIdAsync(int idPlanning);

        Task<IEnumerable<PlanningVisiteDto>> GetPlanningByDelegueAsync(int idDelegue);

        Task<IEnumerable<PlanningVisiteDto>> GetPlanningsByDateRangeAsync(int idDelegue,DateTime startDate,
                                                                          DateTime endDate);

        Task<IEnumerable<PlanningVisiteDto>> GetPlanningByDelegueAndDateAsync(int idDelegue,DateTime date);

        Task<bool> DeletePlanningAsync(int idPlanning);

        // ================================
        // 🔹 LOGIQUE MÉTIER
        // ================================

        // Vérifier conflit horaire
        Task<bool> CheckPlanningConflictAsync(int idDelegue,DateTime debut,DateTime fin);

        // Validation du planning (EnAttente → Confirmé)
        Task<bool> ValidatePlanningAsync(int idPlanning);

    }
}
