using Cynapharm_Mobile.Models.Field;
using RegionModel = Cynapharm_Mobile.Models.Field.Region;

namespace Cynapharm_Mobile.Services;

public class KpiService
{
    private readonly ApiService _api;
    public KpiService(ApiService api) { _api = api; }

    /// <summary>
    /// Returns objectifs for the current user.
    /// SUPERVISEUR/ADMIN: all objectifs via GET fields/objectifs.
    /// DELEGUE: objectifs filtered by delegue via GET fields/objectifs/by-delegue/{id}.
    /// Other roles: empty list.
    /// </summary>
    public async Task<List<Objectif>?> GetObjectifsAsync()
    {
        var role      = await SecureStorage.GetAsync(StorageKeys.UserRole);
        var userIdStr = await SecureStorage.GetAsync(StorageKeys.UserId);

        if (role is "SUPERVISEUR" or "ADMIN")
            return await _api.GetAsync<List<Objectif>>("fields/objectifs");

        if (role == "DELEGUE" && int.TryParse(userIdStr, out var userId))
            return await _api.GetAsync<List<Objectif>>($"fields/objectifs/by-delegue/{userId}");

        return new List<Objectif>();
    }

    /// <summary>
    /// Returns KPI performance indicators for the current user.
    /// Endpoint: GET fields/kpi/performance/{idUser}?debut=yyyy-MM-dd&fin=yyyy-MM-dd
    /// Returns list of PerformanceDto matching backend structure.
    /// Backend handles authorization per role — no client-side role restriction.
    /// </summary>
    public async Task<List<PerformanceDto>?> GetPerformanceAsync(DateTime? debut = null, DateTime? fin = null)
    {
        var userIdStr = await SecureStorage.GetAsync(StorageKeys.UserId);
        if (!int.TryParse(userIdStr, out var userId))
            return new List<PerformanceDto>();

        var debut_ = debut ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var fin_   = fin   ?? DateTime.Now;
        return await _api.GetAsync<List<PerformanceDto>>(
            $"fields/kpi/performance/{userId}?debut={debut_:yyyy-MM-dd}&fin={fin_:yyyy-MM-dd}");
    }

    /// <summary>
    /// Conversion rate (%) for a delegue over a date range.
    /// Endpoint: GET fields/kpi/taux-conversion/{idDelegue}?debut=yyyy-MM-dd&fin=yyyy-MM-dd
    /// </summary>
    public Task<double?> GetTauxConversionAsync(int idDelegue, DateTime debut, DateTime fin)
        => _api.GetAsync<double?>(
            $"fields/kpi/taux-conversion/{idDelegue}?debut={debut:yyyy-MM-dd}&fin={fin:yyyy-MM-dd}");

    public Task<List<RegionModel>?> GetRegionsAsync()
        => _api.GetAsync<List<RegionModel>>("fields/regions");

    /// <summary>
    /// Returns the region(s) assigned to a specific delegate.
    /// Endpoint: GET fields/regions/by-delegue/{idDelegue}
    /// </summary>
    public Task<List<RegionModel>?> GetRegionsByDelegueAsync(int idDelegue)
        => _api.GetAsync<List<RegionModel>>($"fields/regions/by-delegue/{idDelegue}");
}
