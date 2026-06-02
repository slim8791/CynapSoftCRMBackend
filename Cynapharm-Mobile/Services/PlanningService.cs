using Cynapharm_Mobile.Models.Field;

namespace Cynapharm_Mobile.Services;

public class PlanningService
{
    private readonly ApiService _api;
    public PlanningService(ApiService api) { _api = api; }

    public async Task<List<Planning>?> GetPlanningAsync(DateTime weekStart)
    {
        var userIdStr = await SecureStorage.GetAsync(StorageKeys.UserId);
        if (!int.TryParse(userIdStr, out var userId)) return null;

        var endDate = weekStart.AddDays(5); // 6 days: Mon→Sat (matches the 6-slot UI)
        return await _api.GetAsync<List<Planning>>(
            $"fields/plannings/by-range?idDelegue={userId}" +
            $"&startDate={weekStart:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");
    }

    public Task<Planning?> CreatePlanningEntryAsync(Planning entry)
        => _api.PostAsync<Planning>("fields/plannings", entry);

    public Task<Planning?> UpdatePlanningEntryAsync(int id, Planning entry)
        => _api.PutAsync<Planning>($"fields/plannings/{id}", entry);

    public Task<Planning?> GetPlanningByIdAsync(int id)
        => _api.GetAsync<Planning>($"fields/plannings/{id}");

    public Task<bool> DeletePlanningEntryAsync(int id)
        => _api.DeleteAsync($"fields/plannings/{id}");
}
