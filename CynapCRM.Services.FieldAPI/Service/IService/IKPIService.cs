using CynapCRM.Services.FieldAPI.Models.Dto;

namespace CynapCRM.Services.FieldAPI.Service.IService
{
    public interface IKPIService
    {


        // ================================
        // 🔥 LOGIQUE MÉTIER AVANCÉE
        // ================================

        // Vérifier si un délégué a déjà une visite à une date donnée

        // Nombre de visites effectuées
        Task<int> GetNombreVisitesAsync(int idDelegue, DateTime debut, DateTime fin);



        // KPI global (visites + objectifs)
        // 🔥 NOUVEAU : historique activité du délégué
        // utilisé pour audit et reporting
        Task<IEnumerable<ActiviteHistoriqueDto>> GetHistoriqueActiviteAsync(int idDelegue);

        // 🔥 NOUVEAU : score de fidélité client
        Task<int> CalculateClientFideliteAsync(int idClient);
    }
}
