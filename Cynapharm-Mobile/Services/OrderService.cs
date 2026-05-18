using Cynapharm_Mobile.Models.Orders;

namespace Cynapharm_Mobile.Services;

public class OrderService
{
    private readonly ApiService _api;
    public OrderService(ApiService api) { _api = api; }

    public Task<List<Order>?> GetOrdersAsync(string? status, int page = 1, int size = 20)
        => _api.GetAsync<List<Order>>($"orders?status={status}&page={page}&size={size}");

    public Task<Order?> GetOrderByIdAsync(int id)
        => _api.GetAsync<Order>($"orders/{id}");

    public Task<List<LigneCommande>?> GetLignesAsync(int orderId)
        => _api.GetAsync<List<LigneCommande>>($"orders/lignes?orderId={orderId}");

    public Task<Order?> CreateOrderAsync(object request)
        => _api.PostAsync<Order>("orders", request);

    public Task<Order?> UpdateOrderStatusAsync(int id, string status)
        => _api.PutAsync<Order>($"orders/{id}/status", new { Status = status });

    public Task<Reclamation?> CreateReclamationAsync(Reclamation reclamation)
        => _api.PostAsync<Reclamation>("orders/reclamations", reclamation);

    public Task<List<Reclamation>?> GetReclamationsAsync(int? orderId)
        => _api.GetAsync<List<Reclamation>>($"orders/reclamations?orderId={orderId}");
}
