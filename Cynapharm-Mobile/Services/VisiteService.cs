using Cynapharm_Mobile.Models.Field;

namespace Cynapharm_Mobile.Services;

public class VisiteService
{
    private readonly ApiService _api;

    public VisiteService(ApiService api) { _api = api; }

    // ── Visites ───────────────────────────────────────────────────────────────

    public async Task<List<Visite>?> GetVisitesAsync(DateTime? from, DateTime? to, string? status)
    {
        var userIdStr = await SecureStorage.GetAsync(StorageKeys.UserId);
        if (!int.TryParse(userIdStr, out var userId)) return null;

        var all = await _api.GetAsync<List<Visite>>($"fields/visites/by-delegue/{userId}");
        if (all == null) return null;

        return all.Where(v =>
            (from   == null || v.DateVisite.Date >= from.Value.Date)  &&
            (to     == null || v.DateVisite.Date <= to.Value.Date)    &&
            (status == null || string.Equals(v.Statut, status, StringComparison.OrdinalIgnoreCase))
        ).ToList();
    }

    public Task<Visite?> GetVisiteByIdAsync(int id)
        => _api.GetAsync<Visite>($"fields/visites/{id}");

    public Task<Visite?> CreateVisiteAsync(Visite visite)
        => _api.PostAsync<Visite>("fields/visites", visite);

    public Task<Visite?> UpdateVisiteAsync(int id, Visite visite)
        => _api.PutAsync<Visite>($"fields/visites/{id}", visite);

    public Task<bool> DeleteVisiteAsync(int id)
        => _api.DeleteAsync($"fields/visites/{id}");

    // ── Rapports ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Submits a visit report to the FieldAPI via the gateway.
    ///
    /// The gateway route is:
    ///   POST /fields/rapports/{everything}  →  POST /api/rapports/{everything}  (port 7002)
    ///
    /// The backend endpoint is POST /api/rapports/createUpdate and expects a
    /// RapportVisiteDto with the field names used by the backend convention
    /// (Commentaire, Id_Visite, Id_User_Delegue, Latitude, Longitude).
    ///
    /// Coordinates are nullable: the backend accepts null when GPS is unavailable.
    /// </summary>
    public async Task<Rapport?> CreateRapportAsync(Rapport rapport)
    {
        // Read the delegate ID stored at login time — required by the backend
        // ownership check: visite.Id_User_Delegue must equal dto.Id_User_Delegue.
        var userIdStr = await SecureStorage.GetAsync(StorageKeys.UserId);
        int.TryParse(userIdStr, out var userId);

        // Build a payload that matches the backend's RapportVisiteDto exactly.
        var payload = new
        {
            Id_Rapport      = rapport.Id,          // 0 = create, >0 = update
            Id_Visite       = rapport.VisiteId,
            Commentaire     = rapport.Contenu,     // mobile uses "Contenu", backend uses "Commentaire"
            Resultat        = rapport.Resultat,
            Id_User_Delegue = userId,
            Latitude        = rapport.Latitude,    // null when GPS was unavailable or refused
            Longitude       = rapport.Longitude
        };

        return await _api.PostAsync<Rapport>("fields/rapports/createUpdate", payload);
    }

    public Task<List<Rapport>?> GetRapportsAsync(int? visiteId)
    {
        // Backend endpoint: GET /api/rapports/by-visite/{idVisite}
        if (visiteId.HasValue && visiteId > 0)
            return _api.GetAsync<List<Rapport>>($"fields/rapports/by-visite/{visiteId}");

        return _api.GetAsync<List<Rapport>>("fields/rapports/all");
    }
}
