using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Models.Documents;
using Cynapharm_Mobile.Models.Orders;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;

namespace Cynapharm_Mobile.ViewModels.Orders;

[QueryProperty(nameof(OrderId), "orderId")]
public partial class OrderDetailViewModel : BaseViewModel
{
    private readonly OrderService    _orderService;
    private readonly DocumentService _documentService;
    private readonly ProductService  _productService;

    [ObservableProperty] private int              _orderId;
    [ObservableProperty] private Order?           _order;
    [ObservableProperty] private string           _reclamationMotif       = string.Empty;
    [ObservableProperty] private string           _reclamationDescription = string.Empty;
    [ObservableProperty] private bool             _showReclamationForm;
    [ObservableProperty] private bool             _hasLinkedDocuments;
    [ObservableProperty] private LigneCommande?   _selectedLigne;

    public ObservableCollection<LigneCommande>   Lignes         { get; } = new();
    public ObservableCollection<DocumentSummary> LinkedFactures { get; } = new();
    public ObservableCollection<DocumentSummary> LinkedBC       { get; } = new();
    public ObservableCollection<DocumentSummary> LinkedBL       { get; } = new();

    public OrderDetailViewModel(
        OrderService    orderService,
        DocumentService documentService,
        ProductService  productService)
    {
        _orderService    = orderService;
        _documentService = documentService;
        _productService  = productService;
        Title = "Commande";
    }

    public bool IsDelivered          => Order?.Statut == 5;
    public bool IsAnnulee            => Order?.Statut == 6;
    public bool CanCancel            => Order?.Statut is 0 or 1 or 2;
    public bool CanCreateReclamation => Order?.Statut is 4 or 5;
    public bool HasReclamations      => Order?.Reclamations?.Count > 0;

    partial void OnOrderIdChanged(int value)
    {
        if (value > 0) _ = LoadAsync();
    }

    partial void OnOrderChanged(Order? value)
    {
        OnPropertyChanged(nameof(IsDelivered));
        OnPropertyChanged(nameof(IsAnnulee));
        OnPropertyChanged(nameof(CanCancel));
        OnPropertyChanged(nameof(CanCreateReclamation));
        OnPropertyChanged(nameof(HasReclamations));
    }

    [RelayCommand]
    private Task LoadAsync() => ExecuteAsync(async () =>
    {
        if (!await CheckConnectivityAsync()) return;

        var order = await _orderService.GetOrderByIdAsync(OrderId);
        if (order == null) return;

        Order = order;
        Title = $"Commande #{order.NumeroCommande}";

        Lignes.Clear();
        var productIds = order.Lignes.Select(l => l.ProductId).Distinct().ToList();
        var productNames = new Dictionary<int, string>();
        foreach (var id in productIds)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product != null)
                productNames[id] = product.Nom;
        }
        foreach (var ligne in order.Lignes)
        {
            ligne.ProductNom = productNames.TryGetValue(ligne.ProductId, out var nom)
                ? nom
                : $"Produit #{ligne.ProductId}";
            Lignes.Add(ligne);
        }

        await LoadLinkedDocumentsAsync(order.Id);
    });

    private async Task LoadLinkedDocumentsAsync(int commandeId)
    {
        LinkedFactures.Clear();
        LinkedBC.Clear();
        LinkedBL.Clear();

        var factures = await _documentService.GetFacturesByCommandeAsync(commandeId);
        if (factures != null)
            foreach (var f in factures)
                LinkedFactures.Add(new DocumentSummary
                {
                    Id      = f.Id,
                    Numero  = f.NumeroFacture,
                    Date    = f.DateFacture,
                    Type    = "FACTURE",
                    Statut  = f.Statut,
                    Montant = f.MontantTTC
                });

        var bcs = await _documentService.GetBCByCommandeAsync(commandeId);
        if (bcs != null)
            foreach (var b in bcs)
                LinkedBC.Add(new DocumentSummary
                {
                    Id      = b.Id,
                    Numero  = b.NumeroBon,
                    Date    = b.DateEmission,
                    Type    = "BC",
                    Statut  = b.Statut,
                    Montant = b.MontantTotal
                });

        var bls = await _documentService.GetBLByCommandeAsync(commandeId);
        if (bls != null)
            foreach (var bl in bls)
                LinkedBL.Add(new DocumentSummary
                {
                    Id     = bl.Id,
                    Numero = bl.NumeroBon,
                    Date   = bl.DateLivraison,
                    Type   = "BL",
                    Statut = bl.Statut
                });

        HasLinkedDocuments = LinkedFactures.Count > 0 || LinkedBC.Count > 0 || LinkedBL.Count > 0;
    }

    protected override Task RetryAsync() => LoadAsync();

    [RelayCommand]
    private Task CancelOrderAsync() => ExecuteAsync(async () =>
    {
        var motif = await Shell.Current.DisplayPromptAsync(
            "Annuler la commande",
            "Veuillez saisir le motif d'annulation :",
            accept: "Confirmer",
            cancel: "Retour",
            placeholder: "Motif...");

        if (motif == null) return;

        await _orderService.CancelOrderAsync(OrderId, motif.Trim());
        await Shell.Current.DisplayAlert(
            "Commande annulée",
            "Votre commande a été annulée avec succès.",
            "OK");
        await LoadAsync();
    });

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
            LigneId      = SelectedLigne?.Id ?? Lignes.FirstOrDefault()?.Id ?? 0,
            Motif        = message,
            DateCreation = DateTime.UtcNow
        });
        ShowReclamationForm    = false;
        ReclamationMotif       = string.Empty;
        ReclamationDescription = string.Empty;
        SelectedLigne          = null;
        await Shell.Current.DisplayAlert("Succès", "Votre réclamation a été soumise.", "OK");
    });
}
