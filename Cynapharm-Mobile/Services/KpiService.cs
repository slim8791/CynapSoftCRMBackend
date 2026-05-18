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
    /// DELEGUE: no accessible endpoint — ObjectifController is ADMIN/SUPERVISEUR only;
    ///          returns an empty list so the page gracefully shows no data.
    /// </summary>
    public async Task<List<Objectif>?> GetObjectifsAsync()
    {
        var role = await SecureStorage.GetAsync(StorageKeys.UserRole);
        if (role is "SUPERVISEUR" or "ADMIN")
            return await _api.GetAsync<List<Objectif>>("fields/objectifs");

        return new List<Objectif>();
    }

    /// <summary>
    /// All KPI controller endpoints are [Authorize(Roles="ADMIN,SUPERVISEUR")] and require
    /// specific idDelegue / date-range params not available on the mobile dashboard.
    /// Returns an empty list; the dashboard uses TodayVisitCount for DELEGUE users instead.
    /// </summary>
    public Task<List<Kpi>?> GetKpisAsync() =>
        Task.FromResult<List<Kpi>?>(new List<Kpi>());

    public Task<List<RegionModel>?> GetRegionsAsync()
        => _api.GetAsync<List<RegionModel>>("fields/regions");
}
