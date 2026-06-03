using CynapCRM.Services.FieldAPI.Models.Dto;

namespace CynapCRM.Services.FieldAPI.Service.IService
{
    public interface IVisiteService
    {
        Task<IEnumerable<VisiteDto>> GetAllVisitesAsync(
    DateTime? startDate = null,
    DateTime? endDate = null);
        Task<VisiteDto?> CreateOrUpdateVisiteAsync(CreateVisiteDto dto);

        Task<VisiteDto?> GetVisiteByIdAsync(int idVisite);

        Task<IEnumerable<VisiteDto>> GetVisitesByDelegueAsync(int idDelegue);

        // Historique des visites reçues par un médecin (consultable par le médecin lui-même)
        Task<IEnumerable<VisiteDto>> GetVisitesByMedecinAsync(int idMedecin);

        Task<IEnumerable<VisiteDto>> GetVisitesByPlanningAsync(int idPlanning);

        Task<bool> DeleteVisiteAsync(int idVisite);

        // business logic
        Task<bool> AffectVisiteToPlanningAsync(int idVisite, int idPlanning);
        // Mark the visit as completed
        Task<bool> CompleteVisiteAsync(int idVisite);

        // Business security: ownership
        Task<bool> IsVisiteOwnedByDelegueAsync(int idVisite, int idDelegue);

        // Démarrer la visite sur le terrain
        Task<VisiteDto?> StartVisiteAsync(int idVisite);

    }
}
