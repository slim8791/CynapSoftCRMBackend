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

    public Task<List<StockDelegue>?> GetStockDelegueAsync()
        => _api.GetAsync<List<StockDelegue>>("inventory/stocks-delegue");

    public Task<List<StockPromo>?> GetStockPromoAsync()
        => _api.GetAsync<List<StockPromo>>("inventory/stocks-promotionnels");

    public Task<object?> GetDistributionAsync()
        => _api.GetAsync<object>("inventory/distributions");

    /// <summary>
    /// Records a sample distribution on the backend.
    /// Gateway: POST /inventory/distributions → InventoryAPI POST /api/distributions
    /// The payload matches the expected DistributionDto on the server.
    /// Coordinates are optional (present when GPS was available during distribution).
    /// </summary>
    public Task<object?> PostDistributionAsync(
        int productId,
        int quantite,
        double? latitude  = null,
        double? longitude = null)
    {
        var payload = new
        {
            ProductId          = productId,
            QuantiteDistribuee = quantite,
            DateDistribution   = DateTime.UtcNow,
            Latitude           = latitude,
            Longitude          = longitude
        };
        return _api.PostAsync<object>("inventory/distributions", payload);
    }
}
