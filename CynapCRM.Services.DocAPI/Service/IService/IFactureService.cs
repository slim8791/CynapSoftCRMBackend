using CynapCRM.Services.DocAPI.Models.Dto;

namespace CynapCRM.Services.DocAPI.Service.IService
{
    public interface IFactureService
    {
        Task<IEnumerable<FactureDto>> GetFacturesByCommandeAsync(
            int idCommande);
        Task<bool> DeleteFactureAsync(int idFacture);
        Task<IEnumerable<FactureDto>> GetAllFacturesAsync(int pageNumber, int pageSize);

        Task<FactureDto?> CreateOrUpdateFactureAsync(FactureDto factureDto);
        Task<FactureDto?> GetFactureByIdAsync(int idFacture);
        Task<IEnumerable<FactureDto>> GetFacturesByClientAsync(int idClient);
        Task<IEnumerable<FactureDto>> GetFacturesByDateAsync(DateTime startDate, DateTime endDate);
    }
}
