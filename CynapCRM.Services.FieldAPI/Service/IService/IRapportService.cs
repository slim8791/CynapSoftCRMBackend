using CynapCRM.Services.FieldAPI.Models.Dto;

namespace CynapCRM.Services.FieldAPI.Service.IService
{
    public interface IRapportService
    {
        // ================================
        // 🔹 RAPPORT
        // ================================

        Task<RapportVisiteDto?> CreateRapportAsync(RapportVisiteDto dto);
        Task<RapportVisiteDto?> GetRapportByVisiteAsync(int idVisite);
        Task<bool> DeleteRapportAsync(int idRapport);

        // Logique métier
        Task<bool> ValidateRapportAsync(int idRapport, int idSuperviseur);
        // 🔥 NOUVEAU : vérifier si on peut créer un rapport
        // (une visite ne doit avoir qu’un seul rapport)
        Task<bool> CanCreateRapportAsync(int idVisite);
        // Vérifier si une visite a déjà un rapport
        Task<bool> HasRapportAsync(int idVisite);
    }
}
