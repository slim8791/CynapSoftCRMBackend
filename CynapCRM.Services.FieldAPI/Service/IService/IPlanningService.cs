using CynapCRM.Services.FieldAPI.Models.Dto;

namespace CynapCRM.Services.FieldAPI.Service.IService
{
    public interface IPlanningService
    {
        // ================================
        // 🔹 PLANNING
        // ================================

        Task<PlanningVisiteDto?> CreateOrUpdatePlanningAsync(PlanningVisiteDto dto);
        Task<PlanningVisiteDto?> GetPlanningByIdAsync(int idPlanning);
        Task<IEnumerable<PlanningVisiteDto>> GetPlanningByDelegueAsync(int idDelegue);
        Task<bool> DeletePlanningAsync(int idPlanning);

        // Logique métier
        Task<bool> ChangePlanningStatusAsync(int idPlanning, string statut);
        // 🔥 NOUVEAU : vérifier conflit de planning (important en production)
        // Empêche qu’un délégué ait plusieurs plannings ou visites au même moment
        Task<bool> CheckPlanningConflictAsync(int idDelegue, DateTime debut, DateTime fin);

        // 🔥 NOUVEAU : validation complète du planning
        // Vérifie cohérence globale (dates, tournées existantes…)
        Task<bool> ValidatePlanningAsync(int idPlanning);
        // 🔥 NOUVEAU : vérifier disponibilité réelle du délégué
        // (évite chevauchement de visites)
        Task<bool> CheckDelegueAvailabilityAsync(int idDelegue, DateTime date);
    }
}
