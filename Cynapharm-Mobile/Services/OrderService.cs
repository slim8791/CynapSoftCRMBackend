using Cynapharm_Mobile.Models.Orders;

namespace Cynapharm_Mobile.Services;

public class OrderService
{
    private readonly ApiService _api;
    public OrderService(ApiService api) { _api = api; }

    public Task<List<Order>?> GetOrdersAsync(string? status, int page = 1, int size = 20)
        => _api.GetAsync<List<Order>>($"orders?status={status}&page={page}&size={size}");

    /// <summary>GET orders/by-status?statut={status}&page={page}&pageSize={size}</summary>
    public Task<List<Order>?> GetOrdersByStatusAsync(string? status, int page = 1, int size = 20)
    {
        var url = $"{ApiRoutes.Orders.ByStatus}?page={page}&pageSize={size}";
        if (!string.IsNullOrEmpty(status)) url += $"&statut={status}";
        return _api.GetAsync<List<Order>>(url);
    }

    /// <summary>GET orders/by-client/{id}?page={page}&pageSize={size}</summary>
    public Task<List<Order>?> GetOrdersByClientAsync(int clientId, int page = 1, int size = 20)
        => _api.GetAsync<List<Order>>($"{ApiRoutes.Orders.ByClient}/{clientId}?page={page}&pageSize={size}");

    public Task<Order?> GetOrderByIdAsync(int id)
        => _api.GetAsync<Order>($"orders/{id}");

    public Task<List<LigneCommande>?> GetLignesAsync(int orderId)
        => _api.GetAsync<List<LigneCommande>>($"orders/lignes?orderId={orderId}");

    public Task<Order?> CreateOrderAsync(object request)
        => _api.PostAsync<Order>("orders", request);

    public Task<Order?> UpdateOrderStatusAsync(int id, string status)
        => _api.PutAsync<Order>($"orders/{id}/status", new { Status = status });

    /// <summary>PUT orders/{id}/cancel?motif={motif}</summary>
    public Task<object?> CancelOrderAsync(int id, string motif)
        => _api.PutAsync<object>(
            $"{string.Format(ApiRoutes.Orders.Cancel, id)}?motif={Uri.EscapeDataString(motif)}",
            new { });

    public Task<Reclamation?> CreateReclamationAsync(Reclamation reclamation)
        => _api.PostAsync<Reclamation>("orders/reclamations", reclamation);

    public Task<List<Reclamation>?> GetReclamationsAsync(int? orderId)
        => _api.GetAsync<List<Reclamation>>($"orders/reclamations?orderId={orderId}");
}
