using CynapCRM.Services.FieldAPI.Models.Dto;

namespace CynapCRM.Services.FieldAPI.Service.IService
{
    public interface IVisiteService
    {
        // ================================
        // 🔹 VISITE
        // ================================

        Task<VisiteDto?> CreateOrUpdateVisiteAsync(VisiteDto dto);
        Task<VisiteDto?> GetVisiteByIdAsync(int idVisite);
        Task<IEnumerable<VisiteDto>> GetVisitesByDelegueAsync(int idDelegue);
        Task<IEnumerable<VisiteDto>> GetVisitesByTourneeAsync(int idTournee);
        Task<bool> DeleteVisiteAsync(int idVisite);

        // Logique métier
        Task<bool> AffectVisiteToTourneeAsync(int idVisite, int idTournee);
        Task<bool> CompleteVisiteAsync(int idVisite);
        // 🔥 NOUVEAU : vérifier appartenance (sécurité métier)
        // Empêche un délégué de modifier les données d’un autre
        Task<bool> IsVisiteOwnedByDelegueAsync(int idVisite, int idDelegue);
    }
}
