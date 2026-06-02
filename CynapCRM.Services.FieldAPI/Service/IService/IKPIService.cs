using CynapCRM.Services.FieldAPI.Models.Dto;

namespace CynapCRM.Services.FieldAPI.Service.IService
{
    public interface IKPIService
    {
        Task<double> GetTauxConversionAsync(
    int idDelegue,
    DateTime debut,
    DateTime fin);
        Task<int> GetNombreVisitesAsync(int idDelegue, DateTime debut, DateTime fin);

        // Check if a visit already exists on a given date.  
        Task<bool> HasVisiteAtDateAsync(int idDelegue, DateTime date);

        // Delegate activity history (audit / reporting)
        Task<IEnumerable<ActiviteHistoriqueDto>> GetHistoriqueActiviteAsync(int idDelegue);

        // Client KPI (loyalty)
        Task<int> CalculateClientFideliteAsync(int idClient);

        // Performance by objective
        Task<IEnumerable<PerformanceDto>> CalculatePerformanceAsync(int idDelegue);

        // Simplified global KPI (dashboard)
        Task<double> GetPerformanceRateAsync(int idDelegue);

    }
}
