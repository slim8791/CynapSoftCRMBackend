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

    [ObservableProperty] private string _documentType    = "facture";
    [ObservableProperty] private int    _selectedTypeIndex;

    // Maps UI type keys to the unified API type parameter
    private static readonly Dictionary<string, string> _apiTypeMap = new()
    {
        { "facture",        "FACTURE" },
        { "bon-commande",   "BC"      },
        { "bon-livraison",  "BL"      },
    };

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

        if (!_apiTypeMap.TryGetValue(DocumentType, out var apiType)) return;

        var role = await SecureStorage.GetAsync(StorageKeys.UserRole);

        // MÉDECIN only accesses marketing documents, not order documents
        if (role == "MEDECIN")
        {
            await Shell.Current.GoToAsync("//products");
            return;
        }

        var isAdmin = role is "ADMIN" or "SUPERVISEUR";

        Documents.Clear();

        if (isAdmin)
        {
            switch (apiType)
            {
                case "FACTURE":
                    var factures = await _documentService.GetFacturesAsync(1, 50);
                    if (factures != null)
                        foreach (var f in factures)
                            Documents.Add(new DocumentSummary { Id = f.Id, Numero = f.NumeroFacture, Date = f.DateFacture, Type = "FACTURE", CommandeId = f.CommandeId });
                    break;
                case "BC":
                    var bcs = await _documentService.GetBonsCommandeAsync(1, 50);
                    if (bcs != null)
                        foreach (var b in bcs)
                            Documents.Add(new DocumentSummary { Id = b.Id, Numero = b.NumeroBon, Date = b.DateEmission, Type = "BC", CommandeId = b.CommandeId });
                    break;
                case "BL":
                    var bls = await _documentService.GetBonsLivraisonAsync(1, 50);
                    if (bls != null)
                        foreach (var b in bls)
                            Documents.Add(new DocumentSummary { Id = b.Id, Numero = b.NumeroBon, Date = b.DateLivraison, Type = "BL", CommandeId = b.CommandeId });
                    break;
            }
        }
        else
        {
            var userIdStr = await SecureStorage.GetAsync(StorageKeys.UserId);
            if (!int.TryParse(userIdStr, out var clientId)) return;

            var docs = await _documentService.GetDocumentsByClientAndTypeAsync(clientId, apiType);
            if (docs != null)
                foreach (var d in docs)
                    Documents.Add(d);
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
    private async Task OpenDocumentAsync(DocumentSummary? doc)
    {
        if (doc == null || string.IsNullOrWhiteSpace(doc.Url)) return;
        try
        {
            await Launcher.OpenAsync(new Uri(doc.Url));
        }
        catch
        {
            await Shell.Current.DisplayAlert("Erreur", "Impossible d'ouvrir le document.", "OK");
        }
    }

    [RelayCommand]
    private void SetTypeIndex(string index)
    {
        if (int.TryParse(index, out var i)) SelectedTypeIndex = i;
    }
}
