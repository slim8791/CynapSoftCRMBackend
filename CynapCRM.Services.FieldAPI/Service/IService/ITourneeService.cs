using CynapCRM.Services.FieldAPI.Models.Dto;

namespace CynapCRM.Services.FieldAPI.Service.IService
{
    public interface ITourneeService
    {
        // ================================
        // 🔹 TOURNÉE
        // ================================

        Task<TourneeDto?> CreateOrUpdateTourneeAsync(TourneeDto dto);
        Task<TourneeDto?> GetTourneeByIdAsync(int idTournee);
        Task<IEnumerable<TourneeDto>> GetTourneesByPlanningAsync(int idPlanning);
        Task<bool> DeleteTourneeAsync(int idTournee);

        // Logique métier
        Task<bool> StartTourneeAsync(int idTournee);
        Task<bool> EndTourneeAsync(int idTournee);
        // 🔥 NOUVEAU : validation métier d’une tournée
        // Vérifie qu’elle contient des visites et qu’elle est cohérente
        Task<bool> ValidateTourneeAsync(int idTournee);

        // 🔥 NOUVEAU : taux de complétion de la tournée
        // KPI : % des visites réalisées
        Task<double> GetTourneeCompletionRateAsync(int idTournee);
    }
}
