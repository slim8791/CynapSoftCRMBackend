using RegionModel = Cynapharm_Mobile.Models.Field.Region;

namespace Cynapharm_Mobile.Services;

public class FieldService
{
    private readonly ApiService _api;
    public FieldService(ApiService api) { _api = api; }

    public Task<List<RegionModel>?> GetAllRegionsAsync()
        => _api.GetAsync<List<RegionModel>>("fields/regions/all");

    public Task<RegionModel?> GetRegionByIdAsync(int idRegion)
        => _api.GetAsync<RegionModel>($"fields/regions/{idRegion}");

    public Task<List<RegionModel>?> GetRegionsBySuperviseurAsync(int idSuperviseur)
        => _api.GetAsync<List<RegionModel>>($"fields/regions/by-superviseur/{idSuperviseur}");

    public Task<List<RegionModel>?> GetRegionsByDelegueAsync(int idDelegue)
        => _api.GetAsync<List<RegionModel>>($"fields/regions/by-delegue/{idDelegue}");
}
