using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Models.Orders;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;

namespace Cynapharm_Mobile.ViewModels.Orders;

[QueryProperty(nameof(OrderId), "orderId")]
public partial class OrderDetailViewModel : BaseViewModel
{
    private readonly OrderService _orderService;

    [ObservableProperty] private int _orderId;
    [ObservableProperty] private Order? _order;
    [ObservableProperty] private string _reclamationMotif = string.Empty;
    [ObservableProperty] private string _reclamationDescription = string.Empty;
    [ObservableProperty] private bool _showReclamationForm;

    public ObservableCollection<LigneCommande> Lignes { get; } = new();

    public OrderDetailViewModel(OrderService orderService)
    {
        _orderService = orderService;
        Title = "Commande";
    }

    public bool IsDelivered => Order?.Statut == "LIVREE";

    partial void OnOrderIdChanged(int value)
    {
        if (value > 0) _ = LoadAsync();
    }

    partial void OnOrderChanged(Order? value)
    {
        OnPropertyChanged(nameof(IsDelivered));
    }

    [RelayCommand]
    private Task LoadAsync() => ExecuteAsync(async () =>
    {
        if (!await CheckConnectivityAsync()) return;
        var order = await _orderService.GetOrderByIdAsync(OrderId);
        if (order != null)
        {
            Order = order;
            Title = $"Commande #{order.NumeroCommande}";
            Lignes.Clear();
            foreach (var l in order.Lignes) Lignes.Add(l);
        }
    });

    protected override Task RetryAsync() => LoadAsync();

    [RelayCommand]
    private void ToggleReclamationForm() => ShowReclamationForm = !ShowReclamationForm;

    [RelayCommand]
    private Task SubmitReclamationAsync() => ExecuteAsync(async () =>
    {
        if (string.IsNullOrWhiteSpace(ReclamationMotif))
        {
            ErrorMessage = "Veuillez saisir un motif.";
            return;
        }
        var message = string.IsNullOrWhiteSpace(ReclamationDescription)
            ? ReclamationMotif
            : $"{ReclamationMotif}: {ReclamationDescription}";

        await _orderService.CreateReclamationAsync(new Reclamation
        {
            CommandeId   = OrderId,
            LigneId      = Lignes.FirstOrDefault()?.Id ?? 0,
            Motif        = message,
            DateCreation = DateTime.UtcNow
        });
        ShowReclamationForm    = false;
        ReclamationMotif       = string.Empty;
        ReclamationDescription = string.Empty;
        await Shell.Current.DisplayAlert("Succès", "Votre réclamation a été soumise.", "OK");
    });
}
