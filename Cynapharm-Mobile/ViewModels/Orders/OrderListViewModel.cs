using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Models.Orders;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;

namespace Cynapharm_Mobile.ViewModels.Orders;

public partial class OrderListViewModel : BaseViewModel
{
    private readonly OrderService _orderService;

    public ObservableCollection<Order> Orders { get; } = new();

    // French display labels for the filter chips
    public List<string> StatusOptions { get; } = new()
    {
        "Tous", "En attente", "Confirmée", "En préparation", "Expédiée", "Livrée", "Annulée"
    };

    // Maps French display label → EtatCommande integer value
    private static readonly Dictionary<string, int?> _statusCodeMap = new()
    {
        { "En attente",     1 },
        { "Confirmée",      2 },
        { "En préparation", 3 },
        { "Expédiée",       4 },
        { "Livrée",         5 },
        { "Annulée",        6 },
    };

    [ObservableProperty] private string _statusFilter = "Tous";
    [ObservableProperty] private bool   _isGrossiste;

    private int  _currentPage = 1;
    private bool _isClient;
    private int  _clientId;

    [ObservableProperty] private bool _hasMore;

    public OrderListViewModel(OrderService orderService)
    {
        _orderService = orderService;
        Title = "Commandes";
    }

    partial void OnStatusFilterChanged(string value) => _ = LoadAsync();

    [RelayCommand]
    private Task LoadAsync() => ExecuteAsync(async () =>
    {
        var role = await SecureStorage.GetAsync(StorageKeys.UserRole) ?? string.Empty;

        // MÉDECIN has no orders — redirect immediately
        if (role == "MEDECIN")
        {
            await Shell.Current.GoToAsync("//products");
            return;
        }

        IsGrossiste = role is "GROSSISTE";
        _isClient   = role is "PHARMACIEN" or "GROSSISTE" or "CLIENT";

        var userIdStr = await SecureStorage.GetAsync(StorageKeys.UserId);
        if (!int.TryParse(userIdStr, out _clientId) || _clientId <= 0)
        {
            if (_isClient)
            {
                ErrorMessage = "Impossible d'identifier votre compte client. Veuillez vous reconnecter.";
                return;
            }
        }

        if (!await CheckConnectivityAsync()) return;
        _currentPage = 1;
        Orders.Clear();

        var result = await FetchPageAsync(_currentPage);
        if (result != null)
        {
            var filtered = result.Where(o => o.Statut != 0).ToList();
            foreach (var o in filtered) Orders.Add(o);
            HasMore = result.Count == 20;
        }
    });

    [RelayCommand]
    private Task LoadMoreAsync()
    {
        if (!HasMore || IsBusy) return Task.CompletedTask;
        _currentPage++;
        return ExecuteUncheckedAsync(async () =>
        {
            var result = await FetchPageAsync(_currentPage);
            if (result != null)
            {
                var filtered = result.Where(o => o.Statut != 0).ToList();
                foreach (var o in filtered) Orders.Add(o);
                HasMore = result.Count == 20;
            }
        });
    }

    private Task<List<Order>?> FetchPageAsync(int page)
    {
        // Convert French display label → EtatCommande int (null = no filter = "Tous")
        _statusCodeMap.TryGetValue(StatusFilter, out var statut);

        // CLIENT/PHARMACIEN/GROSSISTE → their own orders, filtered by status
        if (_isClient && _clientId > 0)
            return _orderService.GetOrdersByClientAsync(_clientId, statut, page, 20);

        // DELEGUE/ADMIN/SUPERVISEUR → all orders via GET /orders
        return _orderService.GetOrdersAsync(statut, page, 20);
    }

    [RelayCommand]
    private Task RefreshAsync() => LoadAsync();

    protected override Task RetryAsync() => LoadAsync();

    [RelayCommand]
    private async Task GoToDetailAsync(Order? order)
    {
        if (order == null) return;
        await Shell.Current.GoToAsync($"//orders/detail?orderId={order.Id}");
    }

    [RelayCommand]
    private async Task CreateOrderAsync()
        => await Shell.Current.GoToAsync("//orders/create");

    [RelayCommand]
    private void SetStatusFilter(string status) => StatusFilter = status;
}
