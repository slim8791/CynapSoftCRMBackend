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
    public List<string> StatusOptions { get; } = new() { "Tous", "EN_ATTENTE", "CONFIRMEE", "LIVREE", "ANNULEE" };

    [ObservableProperty] private string _statusFilter = "Tous";
    [ObservableProperty] private bool   _isGrossiste;

    private int _currentPage = 1;
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
        IsGrossiste = role is "GROSSISTE";

        if (!await CheckConnectivityAsync()) return;
        _currentPage = 1;
        Orders.Clear();
        var status = StatusFilter == "Tous" ? null : StatusFilter;
        var result = await _orderService.GetOrdersAsync(status, _currentPage, 20);
        if (result != null)
        {
            foreach (var o in result) Orders.Add(o);
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
            var status = StatusFilter == "Tous" ? null : StatusFilter;
            var result = await _orderService.GetOrdersAsync(status, _currentPage, 20);
            if (result != null)
            {
                foreach (var o in result) Orders.Add(o);
                HasMore = result.Count == 20;
            }
        });
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
