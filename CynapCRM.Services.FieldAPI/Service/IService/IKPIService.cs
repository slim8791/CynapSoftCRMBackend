using CynapCRM.Services.FieldAPI.Models.Dto;

namespace CynapCRM.Services.FieldAPI.Service.IService
{
    public interface IKPIService
    {

        Task<int> GetNombreVisitesAsync(int idDelegue, DateTime debut, DateTime fin);

        // Vérifier s’il existe déjà une visite à une date donnée
        Task<bool> HasVisiteAtDateAsync(int idDelegue, DateTime date);

        // Historique d’activité du délégué (audit / reporting)
        Task<IEnumerable<ActiviteHistoriqueDto>> GetHistoriqueActiviteAsync(int idDelegue);

        // KPI client (fidélité)
        Task<int> CalculateClientFideliteAsync(int idClient);

        // Performance par objectif
        Task<IEnumerable<PerformanceDto>> CalculatePerformanceAsync(int idDelegue);

        // KPI global simplifié (dashboard)
        Task<double> GetPerformanceRateAsync(int idDelegue);

    }
}
