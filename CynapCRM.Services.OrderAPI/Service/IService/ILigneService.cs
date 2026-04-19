using CynapCRM.Services.OrderAPI.Models.Dto;

namespace CynapCRM.Services.OrderAPI.Service.IService
{
    public interface ILigneService
    {
        //  LIGNES DE COMMANDE
        Task<LigneCommandeDto?> CreateOrUpdateLigneCommandeAsync(LigneCommandeDto ligneDto);

        Task<bool> RemoveLigneCommandeAsync(int ligneId);
    }
}
