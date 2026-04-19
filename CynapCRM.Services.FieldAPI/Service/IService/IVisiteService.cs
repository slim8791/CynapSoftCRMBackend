using CynapCRM.Services.FieldAPI.Models.Dto;

namespace CynapCRM.Services.FieldAPI.Service.IService
{
    public interface IVisiteService
    {

        // ================================
        // 🔹 VISITES
        // ================================

        Task<VisiteDto?> CreateOrUpdateVisiteAsync(VisiteDto dto);

        Task<VisiteDto?> GetVisiteByIdAsync(int idVisite);

        Task<IEnumerable<VisiteDto>> GetVisitesByDelegueAsync(int idDelegue);

        // ✅ NOUVEAU : logique Planning (remplace Tournee)
        Task<IEnumerable<VisiteDto>> GetVisitesByPlanningAsync(int idPlanning);

        Task<bool> DeleteVisiteAsync(int idVisite);

        // ================================
        // 🔹 LOGIQUE MÉTIER
        // ================================
        Task<bool> AffectVisiteToPlanningAsync(int idVisite, int idPlanning);
        // Marquer la visite comme terminée
        Task<bool> CompleteVisiteAsync(int idVisite);

        // Sécurité métier : ownership
        Task<bool> IsVisiteOwnedByDelegueAsync(int idVisite, int idDelegue);

    }
}
