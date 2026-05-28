using Cynapharm_Mobile.Models.Orders;

namespace Cynapharm_Mobile.Services;

public class OrderService
{
    private readonly ApiService _api;
    public OrderService(ApiService api) { _api = api; }

    /// <summary>GET orders?page={page}&amp;pageSize={pageSize}&amp;statut={statut} — DELEGUE/ADMIN/SUPERVISEUR</summary>
    public Task<List<Order>?> GetOrdersAsync(int? statut = null, int page = 1, int pageSize = 20)
    {
        var url = $"orders?page={page}&pageSize={pageSize}";
        if (statut.HasValue) url += $"&statut={statut.Value}";
        return _api.GetAsync<List<Order>>(url);
    }

    /// <summary>GET orders/by-status?statut={statut}&amp;page={page}&amp;pageSize={size}</summary>
    public Task<List<Order>?> GetOrdersByStatusAsync(int? statut, int page = 1, int size = 20)
    {
        var url = $"{ApiRoutes.Orders.ByStatus}?page={page}&pageSize={size}";
        if (statut.HasValue) url += $"&statut={statut.Value}";
        return _api.GetAsync<List<Order>>(url);
    }

    /// <summary>GET orders/by-client/{id}?page={page}&amp;pageSize={size}[&amp;statut={statut}]</summary>
    public Task<List<Order>?> GetOrdersByClientAsync(int clientId, int? statut = null, int page = 1, int size = 20)
    {
        var url = $"{ApiRoutes.Orders.ByClient}/{clientId}?page={page}&pageSize={size}";
        if (statut.HasValue) url += $"&statut={statut.Value}";
        return _api.GetAsync<List<Order>>(url);
    }

    public Task<Order?> GetOrderByIdAsync(int id)
        => _api.GetAsync<Order>($"orders/{id}");

    public Task<List<LigneCommande>?> GetLignesAsync(int orderId)
        => _api.GetAsync<List<LigneCommande>>($"orders/lignes?orderId={orderId}");

    public Task<Order?> CreateOrderAsync(object request)
        => _api.PostAsync<Order>("orders", request);

    public Task<Order?> UpdateOrderStatusAsync(int id, string status)
        => _api.PutAsync<Order>($"orders/{id}/status", new { NouveauStatut = status });

    /// <summary>PUT orders/{id}/cancel?motif={motif}</summary>
    public Task<object?> CancelOrderAsync(int id, string motif)
        => _api.PutAsync<object>(
            $"{string.Format(ApiRoutes.Orders.Cancel, id)}?motif={Uri.EscapeDataString(motif)}",
            new { });

    public Task<Reclamation?> CreateReclamationAsync(Reclamation reclamation)
        => _api.PostAsync<Reclamation>("orders/reclamations", reclamation);

    public Task<List<Reclamation>?> GetReclamationsAsync(int? orderId)
        => _api.GetAsync<List<Reclamation>>($"orders/reclamations/by-commande/{orderId}");

    /// <summary>GET orders/reclamations/by-client/{clientId} — all reclamations for a client</summary>
    public Task<List<Reclamation>?> GetReclamationsByClientAsync(int clientId)
        => _api.GetAsync<List<Reclamation>>($"orders/reclamations/by-client/{clientId}");
}
