using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Models.Auth;
using Cynapharm_Mobile.Models.Field;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;

namespace Cynapharm_Mobile.ViewModels.Clients;

public partial class ClientDetailViewModel : BaseViewModel, IQueryAttributable
{
    private readonly UserService _userSvc;
    private readonly VisiteService _visiteSvc;

    [ObservableProperty] private string _clientName = string.Empty;
    [ObservableProperty] private string _clientEmail = string.Empty;
    [ObservableProperty] private string _clientTelephone = string.Empty;
    [ObservableProperty] private string _clientAdresse = string.Empty;
    [ObservableProperty] private string _clientType = string.Empty;
    [ObservableProperty] private string _clientInitials = "?";

    public ObservableCollection<Visite> Visites { get; } = new();

    [ObservableProperty]
    private bool _hasVisites;

    public int ClientId { get; private set; }

    public ClientDetailViewModel(UserService userSvc, VisiteService visiteSvc)
    {
        _userSvc = userSvc;
        _visiteSvc = visiteSvc;
        Title = "Détail client";
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("clientId", out var idObj) && int.TryParse(idObj?.ToString(), out var id))
        {
            ClientId = id;
            _ = LoadAsync();
        }
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        await ExecuteAsync(async () =>
        {
            // Use GetUsersByRoleAsync to avoid 403 for DELEGUE
            // (GetUserByIdAsync is ADMIN+SUPERVISEUR only until backend is republished)
            var clients = await _userSvc.GetUsersByRoleAsync("CLIENT");
            var client = clients?.FirstOrDefault(c => c.Id == ClientId);

            if (client == null)
            {
                ErrorMessage = "Client introuvable.";
                return;
            }

            ClientName      = client.Name ?? string.Empty;
            ClientEmail     = client.Email ?? string.Empty;
            ClientTelephone = client.PhoneNumber ?? string.Empty;
            ClientAdresse   = client.Adresse ?? string.Empty;
            ClientType      = client.TypeClient ?? client.Role ?? "CLIENT";
            ClientInitials  = BuildInitials(client.Name);

            // Load visit history — filter delegue's visits client-side
            var userIdStr = await SecureStorage.GetAsync(StorageKeys.UserId);
            if (int.TryParse(userIdStr, out _))
            {
                var allVisites = await _visiteSvc.GetVisitesAsync(null, null, null);
                Visites.Clear();
                foreach (var v in allVisites?.Where(v =>
                    v.IdPharmacien == ClientId ||
                    v.IdMedecin    == ClientId) ?? Enumerable.Empty<Visite>())
                    Visites.Add(v);
                HasVisites = Visites.Count > 0;
            }
        });
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    private static string BuildInitials(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "?";
        var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2
            ? $"{parts[0][0]}{parts[1][0]}".ToUpperInvariant()
            : name[0].ToString().ToUpperInvariant();
    }

    protected override Task RetryAsync() => LoadAsync();
}
