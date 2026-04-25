using CynapCRM.Services.FieldAPI.Models.Dto;

namespace CynapCRM.Services.FieldAPI.Service.IService
{
    public interface IObjectifService
    {
        Task<IEnumerable<ObjectifDelegueDto>> GetAllObjectifsAsync();

        Task<ObjectifDelegueDto?> CreateOrUpdateObjectifAsync(ObjectifDelegueDto dto);

        Task<IEnumerable<ObjectifDelegueDto>> GetObjectifsByDelegueAsync(int idDelegue);
        Task<ObjectifDelegueDto?> GetObjectifsByIdAsync(int idObjectif);


        Task<bool> DeleteObjectifAsync(int idObjectif);
        Task<bool> UpdateObjectifValueAsync(int idObjectif, int nouvelleValeur);

        

    }
}
