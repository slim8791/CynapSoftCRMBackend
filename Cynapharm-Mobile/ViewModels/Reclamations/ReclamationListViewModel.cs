using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Models.Orders;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;

namespace Cynapharm_Mobile.ViewModels.Reclamations;

public partial class ReclamationListViewModel : BaseViewModel
{
    private readonly OrderService _orderService;

    public ObservableCollection<Reclamation> Reclamations { get; } = new();

    public ReclamationListViewModel(OrderService orderService)
    {
        _orderService = orderService;
        Title = "Mes réclamations";
    }

    [RelayCommand]
    private Task LoadAsync() => ExecuteAsync(async () =>
    {
        if (!await CheckConnectivityAsync()) return;

        var userIdStr = await SecureStorage.GetAsync(StorageKeys.UserId);
        if (!int.TryParse(userIdStr, out var clientId) || clientId <= 0) return;

        var list = await _orderService.GetReclamationsByClientAsync(clientId);
        Reclamations.Clear();
        if (list != null)
            foreach (var r in list)
                Reclamations.Add(r);
    });

    [RelayCommand]
    private Task RefreshAsync() => LoadAsync();

    protected override Task RetryAsync() => LoadAsync();
}
