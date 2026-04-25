using CynapCRM.Services.FieldAPI.Models.Dto;

namespace CynapCRM.Services.FieldAPI.Service.IService
{
    public interface IRapportService
    {

        Task<IEnumerable<RapportVisiteDto>> GetAllRapportsAsync();

        Task<RapportVisiteDto?> GetRapportByIdAsync(int idRapport);

        Task<RapportVisiteDto?> CreateOrUpdateRapportAsync(RapportVisiteDto dto);
        Task<RapportVisiteDto?> GetRapportByVisiteAsync(int idVisite);
        Task<bool> DeleteRapportAsync(int idRapport);
        Task<bool> ValidateRapportAsync(int idRapport, int idSuperviseur);
        Task<bool> CanCreateRapportAsync(int idVisite);
        Task<bool> HasRapportAsync(int idVisite);
    }
}
