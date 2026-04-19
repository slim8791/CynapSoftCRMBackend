using CynapCRM.Services.FieldAPI.Models.Dto;

namespace CynapCRM.Services.FieldAPI.Service.IService
{
    public interface IObjectifService
    {

        // ================================
        // 🔹 OBJECTIFS
        // ================================

        Task<ObjectifDelegueDto?> CreateOrUpdateObjectifAsync(ObjectifDelegueDto dto);

        // ✅ Pluriel (plus réaliste)
        Task<IEnumerable<ObjectifDelegueDto>> GetObjectifsByDelegueAsync(int idDelegue);

        Task<bool> DeleteObjectifAsync(int idObjectif);

        // ================================
        // 🔹 LOGIQUE MÉTIER / KPI
        // ================================

        // Mise à jour de la valeur réalisée (batch / calcul automatique)
        Task<bool> UpdateObjectifValueAsync(int idObjectif, int nouvelleValeur);

        

    }
}
