using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Models.Documents;
using Cynapharm_Mobile.Models.Orders;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;

namespace Cynapharm_Mobile.ViewModels.Documents;

[QueryProperty(nameof(DocumentType), "documentType")]
[QueryProperty(nameof(DocumentId),   "documentId")]
public partial class DocumentDetailViewModel : BaseViewModel
{
    private readonly DocumentService _documentService;
    private readonly OrderService _orderService;
    private readonly ProductService _productService;

    [ObservableProperty] private string _documentType = string.Empty;
    [ObservableProperty] private int    _documentId;

    [ObservableProperty] private Facture?     _facture;
    [ObservableProperty] private BonCommande? _bonCommande;
    [ObservableProperty] private BonLivraison? _bonLivraison;

    // Order linked to the document — source of the line items and amounts.
    // (BC/BL store no amounts server-side; everything lives on the Commande.)
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OrderMontantHT))]
    [NotifyPropertyChangedFor(nameof(OrderTVA))]
    [NotifyPropertyChangedFor(nameof(OrderMontantTTC))]
    private Order? _order;

    public ObservableCollection<LigneCommande> Lignes { get; } = new();

    [ObservableProperty] private bool _hasLignes;

    public decimal OrderMontantHT  => Order?.MontantTotal ?? 0m;
    public decimal OrderMontantTTC => Order?.MontantTTC   ?? 0m;
    public decimal OrderTVA        => OrderMontantTTC - OrderMontantHT;

    public bool IsFacture      => string.Equals(DocumentType, "facture",      StringComparison.OrdinalIgnoreCase);
    public bool IsBonCommande  => string.Equals(DocumentType, "bon-commande", StringComparison.OrdinalIgnoreCase)
                               || string.Equals(DocumentType, "BC",           StringComparison.OrdinalIgnoreCase);
    public bool IsBonLivraison => string.Equals(DocumentType, "bon-livraison", StringComparison.OrdinalIgnoreCase)
                               || string.Equals(DocumentType, "BL",            StringComparison.OrdinalIgnoreCase);

    public DocumentDetailViewModel(
        DocumentService documentService,
        OrderService orderService,
        ProductService productService)
    {
        _documentService = documentService;
        _orderService = orderService;
        _productService = productService;
        Title = "Document";
    }

    partial void OnDocumentIdChanged(int value)
    {
        if (value > 0 && !string.IsNullOrEmpty(DocumentType)) _ = LoadAsync();
    }

    partial void OnDocumentTypeChanged(string value)
    {
        OnPropertyChanged(nameof(IsFacture));
        OnPropertyChanged(nameof(IsBonCommande));
        OnPropertyChanged(nameof(IsBonLivraison));
        if (DocumentId > 0 && !string.IsNullOrEmpty(value)) _ = LoadAsync();
    }

    [RelayCommand]
    private Task LoadAsync() => ExecuteAsync(async () =>
    {
        if (!await CheckConnectivityAsync()) return;

        int commandeId = 0;
        switch (DocumentType.ToLowerInvariant())
        {
            case "facture":
                Facture = await _documentService.GetFactureByIdAsync(DocumentId);
                if (Facture != null) { Title = $"Facture {Facture.NumeroFacture}"; commandeId = Facture.CommandeId; }
                break;
            case "bon-commande":
            case "bc":
                BonCommande = await _documentService.GetBonCommandeByIdAsync(DocumentId);
                if (BonCommande != null) { Title = $"BC {BonCommande.NumeroBon}"; commandeId = BonCommande.CommandeId; }
                break;
            case "bon-livraison":
            case "bl":
                BonLivraison = await _documentService.GetBonLivraisonByIdAsync(DocumentId);
                if (BonLivraison != null) { Title = $"BL {BonLivraison.NumeroBon}"; commandeId = BonLivraison.CommandeId; }
                break;
        }

        // The document only carries metadata; the line items + amounts come from
        // the linked order (orders/{id} already includes the lignes).
        await LoadOrderAsync(commandeId);
    });

    private async Task LoadOrderAsync(int commandeId)
    {
        Lignes.Clear();
        Order = null;
        HasLignes = false;

        if (commandeId <= 0) return;

        Order = await _orderService.GetOrderByIdAsync(commandeId);
        if (Order?.Lignes is not { Count: > 0 } lignes) return;

        // Resolve product names (the order lines only carry the product id).
        var productNames = new Dictionary<int, string>();
        foreach (var id in lignes.Select(l => l.ProductId).Distinct())
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product != null) productNames[id] = product.Nom;
        }

        foreach (var ligne in lignes)
        {
            if (productNames.TryGetValue(ligne.ProductId, out var nom))
                ligne.ProductNom = nom;
            Lignes.Add(ligne);
        }
        HasLignes = true;
    }

    protected override Task RetryAsync() => LoadAsync();

    private string? CloudinaryUrl => DocumentType.ToLowerInvariant() switch
    {
        "facture"               => Facture?.CloudinaryUrl,
        "bon-commande" or "bc"  => BonCommande?.CloudinaryUrl,
        "bon-livraison" or "bl" => BonLivraison?.CloudinaryUrl,
        _                       => null
    };

    private string DocumentFileName => DocumentType.ToLowerInvariant() switch
    {
        "facture"               => $"Facture_{Facture?.NumeroFacture}.pdf",
        "bon-commande" or "bc"  => $"BC_{BonCommande?.NumeroBon}.pdf",
        "bon-livraison" or "bl" => $"BL_{BonLivraison?.NumeroBon}.pdf",
        _                       => "document.pdf"
    };

    // Shown when the PDF has not been generated/uploaded yet (CloudinaryUrl is empty).
    private const string PdfNotReadyMessage =
        "Le document n'est pas encore généré. Veuillez réessayer dans quelques instants.";

    [RelayCommand]
    private Task DownloadAsync() => ExecuteAsync(async () =>
    {
        var url = CloudinaryUrl;

        // No PDF stored yet — be explicit instead of showing a confusing fallback.
        // All the document data is already visible on this page (loaded from the order).
        if (string.IsNullOrEmpty(url))
        {
            await ShowErrorAlertAsync("Document indisponible", PdfNotReadyMessage);
            return;
        }

        var bytes = await _documentService.DownloadFileAsync(url);
        if (bytes is null || bytes.Length == 0)
        {
            await ShowErrorAlertAsync("Document indisponible", PdfNotReadyMessage);
            return;
        }

        var pdfPath = Path.Combine(FileSystem.CacheDirectory, DocumentFileName);
        await File.WriteAllBytesAsync(pdfPath, bytes);
        await Launcher.OpenAsync(
            new OpenFileRequest(DocumentFileName, new ReadOnlyFile(pdfPath, "application/pdf")));
    });
}
