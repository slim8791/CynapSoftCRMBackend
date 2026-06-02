using CynapCRM.Services.FieldAPI.Models.Dto;

namespace CynapCRM.Services.FieldAPI.Service.IService
{
    public interface IPlanningService
    {
        Task<IEnumerable<PlanningVisiteDto>> GetAllPlanningsAsync(
    DateTime? startDate = null,
    DateTime? endDate = null);
        Task<PlanningVisiteDto?> CreateOrUpdatePlanningAsync(PlanningVisiteDto dto);

        Task<PlanningVisiteDto?> GetPlanningByIdAsync(int idPlanning);

        Task<IEnumerable<PlanningVisiteDto>> GetPlanningByDelegueAsync(int idDelegue);

        Task<IEnumerable<PlanningVisiteDto>> GetPlanningsByDateRangeAsync(int idDelegue,DateTime startDate,
                                                                          DateTime endDate);

        Task<IEnumerable<PlanningVisiteDto>> GetPlanningByDelegueAndDateAsync(int idDelegue,DateTime date);

        Task<bool> DeletePlanningAsync(int idPlanning);

        // Check time zone conflict
        Task<bool> CheckPlanningConflictAsync(int idDelegue,DateTime debut,DateTime fin,
            int? excludePlanningId = null);
        // Validate planning (Pending to Confirmed)
        Task<bool> ValidatePlanningAsync(int idPlanning);

    }
}
