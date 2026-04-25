using CynapCRM.Services.OrderAPI.Models.Dto;

namespace CynapCRM.Services.OrderAPI.Service.IService
{
    public interface ILigneService
    {
        Task<LigneCommandeDto?> CreateOrUpdateLigneCommandeAsync(CreateOrUpdateLigneCommandeDto ligneDto);

        Task<bool> RemoveLigneCommandeAsync(int ligneId);
    }
}
