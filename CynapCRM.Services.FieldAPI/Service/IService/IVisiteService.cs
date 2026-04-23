using CynapCRM.Services.FieldAPI.Models.Dto;

namespace CynapCRM.Services.FieldAPI.Service.IService
{
    public interface IVisiteService
    {

        Task<VisiteDto?> CreateOrUpdateVisiteAsync(CreateVisiteDto dto);

        Task<VisiteDto?> GetVisiteByIdAsync(int idVisite);

        Task<IEnumerable<VisiteDto>> GetVisitesByDelegueAsync(int idDelegue);

        Task<IEnumerable<VisiteDto>> GetVisitesByPlanningAsync(int idPlanning);

        Task<bool> DeleteVisiteAsync(int idVisite);

        // business logic
        Task<bool> AffectVisiteToPlanningAsync(int idVisite, int idPlanning);
        // Mark the visit as completed
        Task<bool> CompleteVisiteAsync(int idVisite);

        // Business security: ownership
        Task<bool> IsVisiteOwnedByDelegueAsync(int idVisite, int idDelegue);

    }
}
