using Cynapharm_Mobile.Models.Inventory;

namespace Cynapharm_Mobile.Services;

public class InventoryService
{
    private readonly ApiService _api;
    public InventoryService(ApiService api) { _api = api; }

    public Task<List<StockMouvement>?> GetStockMouvementsAsync(int? productId, DateTime? from)
    {
        var query = "inventory/stock-movements?";
        if (productId.HasValue) query += $"productId={productId}&";
        if (from.HasValue) query += $"from={from.Value:yyyy-MM-dd}&";
        return _api.GetAsync<List<StockMouvement>>(query.TrimEnd('&', '?'));
    }

    public async Task<List<StockDelegue>?> GetStockDelegueAsync()
    {
        var userIdStr = await SecureStorage.GetAsync(StorageKeys.UserId);
        if (!int.TryParse(userIdStr, out var userId)) return null;
        return await _api.GetAsync<List<StockDelegue>>($"inventory/stocks-delegue/by-delegue/{userId}");
    }

    public Task<List<StockPromo>?> GetStockPromoAsync()
        => _api.GetAsync<List<StockPromo>>("inventory/stocks-promotionnels");

    public Task<object?> GetDistributionAsync()
        => _api.GetAsync<object>("inventory/distributions");

    /// <summary>
    /// Records a sample distribution on the backend.
    /// Gateway: POST /inventory/distributions → InventoryAPI POST /api/distributions
    /// Exactly one of idMedecin or idPharmacien must be non-null.
    /// </summary>
    public async Task<object?> PostDistributionAsync(
        int stockId,
        int quantite,
        string numeroLot,
        int? idMedecin    = null,
        int? idPharmacien = null)
    {
        var userIdStr = await SecureStorage.GetAsync(StorageKeys.UserId);
        int.TryParse(userIdStr, out var userId);

        var dto = new
        {
            id_Distribution  = 0,
            id_Delegue       = userId,
            id_Medecin       = idMedecin,
            id_Pharmacien    = idPharmacien,
            id_Stock         = stockId,
            qte              = quantite,
            numeroLot        = numeroLot,
            dateDistribution = (DateTime?)null
        };
        return await _api.PostAsync<object>("inventory/distributions", dto);
    }

    // ── New endpoints ─────────────────────────────────────────────────────────

    /// <summary>
    /// GET inventory/inventory-business/summary/{idDelegue}
    /// Returns stock KPIs for the delegate's dashboard.
    /// </summary>
    public Task<StockSummaryDto?> GetStockSummaryAsync(int idDelegue)
        => _api.GetAsync<StockSummaryDto>($"{ApiRoutes.Inventory.StockSummary}/{idDelegue}");

    /// <summary>
    /// GET inventory/stock-movements/by-delegue/{idDelegue}
    /// Returns all stock movements for the delegate's History tab.
    /// </summary>
    public Task<List<StockMouvement>?> GetMovementsByDelegueAsync(int idDelegue)
        => _api.GetAsync<List<StockMouvement>>($"{ApiRoutes.Inventory.MovementsByDelegue}/{idDelegue}");
}
