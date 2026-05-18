using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Models.Documents;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;

namespace Cynapharm_Mobile.ViewModels.Documents;

[QueryProperty(nameof(DocumentType), "documentType")]
[QueryProperty(nameof(DocumentId),   "documentId")]
public partial class DocumentDetailViewModel : BaseViewModel
{
    private readonly DocumentService _documentService;

    [ObservableProperty] private string _documentType = string.Empty;
    [ObservableProperty] private int    _documentId;

    [ObservableProperty] private Facture?     _facture;
    [ObservableProperty] private BonCommande? _bonCommande;
    [ObservableProperty] private BonLivraison? _bonLivraison;

    public bool IsFacture     => DocumentType == "facture";
    public bool IsBonCommande => DocumentType == "bon-commande";
    public bool IsBonLivraison => DocumentType == "bon-livraison";

    public DocumentDetailViewModel(DocumentService documentService)
    {
        _documentService = documentService;
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
        switch (DocumentType)
        {
            case "facture":
                Facture = await _documentService.GetFactureByIdAsync(DocumentId);
                if (Facture != null) Title = $"Facture {Facture.NumeroFacture}";
                break;
            case "bon-commande":
                BonCommande = await _documentService.GetBonCommandeByIdAsync(DocumentId);
                if (BonCommande != null) Title = $"BC {BonCommande.NumeroBon}";
                break;
            case "bon-livraison":
                BonLivraison = await _documentService.GetBonLivraisonByIdAsync(DocumentId);
                if (BonLivraison != null) Title = $"BL {BonLivraison.NumeroBon}";
                break;
        }
    });

    protected override Task RetryAsync() => LoadAsync();

    [RelayCommand]
    private Task ShareAsync() => ExecuteAsync(async () =>
    {
        var text = DocumentType switch
        {
            "facture"       when Facture     != null => $"Facture {Facture.NumeroFacture} — {Facture.MontantTTC:C2} — {Facture.Statut}",
            "bon-commande"  when BonCommande != null => $"Bon de commande {BonCommande.NumeroBon} — {BonCommande.MontantTotal:C2}",
            "bon-livraison" when BonLivraison != null => $"Bon de livraison {BonLivraison.NumeroBon} — {BonLivraison.Statut}",
            _                                        => "Document Cynapharm"
        };
        await Share.RequestAsync(new ShareTextRequest { Text = text, Title = Title });
    });
}
