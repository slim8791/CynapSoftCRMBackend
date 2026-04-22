using CynapCRM.Services.OrderAPI.Models.Dto;

namespace CynapCRM.Services.OrderAPI.Service.IService
{
    public interface IReclamationService
    {

        Task<IEnumerable<ReclamationDto>> GetAllReclamationsAsync();

        Task<IEnumerable<ReclamationDto>> GetReclamationsByOrderAsync(int orderId);

        Task<IEnumerable<ReclamationDto>> GetReclamationsByClientAsync(int idClient);

        Task<ReclamationDto?> GetReclamationByIdAsync(int idReclamation);

        Task<ReclamationDto?> CreateUpdateReclamationAsync(ReclamationDto dto);

        Task<bool> UpdateReclamationStatusAsync(int reclamationId, StatutReclamation newStatus);

        Task<bool> DeleteReclamationAsync(int reclamationId);
    }
}
