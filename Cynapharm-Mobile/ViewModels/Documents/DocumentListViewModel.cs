using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Models.Documents;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;

namespace Cynapharm_Mobile.ViewModels.Documents;

public partial class DocumentListViewModel : BaseViewModel
{
    private readonly DocumentService _documentService;

    public ObservableCollection<DocumentSummary> Documents { get; } = new();
    public List<string> TypeOptions { get; } = new() { "facture", "bon-commande", "bon-livraison" };
    public List<string> TypeLabels  { get; } = new() { "Factures", "Bons de commande", "Bons de livraison" };

    [ObservableProperty] private string _documentType     = "facture";
    [ObservableProperty] private int    _selectedTypeIndex;

    public DocumentListViewModel(DocumentService documentService)
    {
        _documentService = documentService;
        Title = "Documents";
    }

    partial void OnSelectedTypeIndexChanged(int value)
    {
        if (value >= 0 && value < TypeOptions.Count)
        {
            DocumentType = TypeOptions[value];
            _ = LoadAsync();
        }
    }

    [RelayCommand]
    private Task LoadAsync() => ExecuteAsync(async () =>
    {
        if (!await CheckConnectivityAsync()) return;
        Documents.Clear();
        switch (DocumentType)
        {
            case "facture":
                var factures = await _documentService.GetFacturesAsync();
                if (factures != null)
                    foreach (var f in factures)
                        Documents.Add(new DocumentSummary
                        {
                            Id     = f.Id,
                            Numero = f.NumeroFacture,
                            Date   = f.DateFacture,
                            Type   = "facture",
                            Statut = f.Statut,
                            Montant = f.MontantTTC
                        });
                break;

            case "bon-commande":
                var bons = await _documentService.GetBonsCommandeAsync();
                if (bons != null)
                    foreach (var b in bons)
                        Documents.Add(new DocumentSummary
                        {
                            Id      = b.Id,
                            Numero  = b.NumeroBon,
                            Date    = b.DateEmission,
                            Type    = "bon-commande",
                            Statut  = b.Statut,
                            Montant = b.MontantTotal
                        });
                break;

            case "bon-livraison":
                var bls = await _documentService.GetBonsLivraisonAsync();
                if (bls != null)
                    foreach (var bl in bls)
                        Documents.Add(new DocumentSummary
                        {
                            Id     = bl.Id,
                            Numero = bl.NumeroBon,
                            Date   = bl.DateLivraison,
                            Type   = "bon-livraison",
                            Statut = bl.Statut
                        });
                break;
        }
    });

    protected override Task RetryAsync() => LoadAsync();

    [RelayCommand]
    private async Task GoToDetailAsync(DocumentSummary? doc)
    {
        if (doc == null) return;
        await Shell.Current.GoToAsync($"//documents/detail?documentType={doc.Type}&documentId={doc.Id}");
    }

    [RelayCommand]
    private void SetTypeIndex(string index)
    {
        if (int.TryParse(index, out var i)) SelectedTypeIndex = i;
    }
}
