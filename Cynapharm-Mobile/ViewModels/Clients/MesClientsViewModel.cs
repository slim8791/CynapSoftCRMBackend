using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Models.Auth;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;

namespace Cynapharm_Mobile.ViewModels.Clients;

public partial class MesClientsViewModel : BaseViewModel
{
    private readonly UserService _userSvc;
    private List<UserListItem> _allClients = new();

    public ObservableCollection<UserListItem> Clients { get; } = new();

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private bool _isEmpty;

    public MesClientsViewModel(UserService userSvc)
    {
        _userSvc = userSvc;
        Title = "Mes clients";
    }

    partial void OnSearchQueryChanged(string value) => ApplyFilter();

    [RelayCommand]
    private async Task LoadAsync()
    {
        await ExecuteAsync(async () =>
        {
            var all = await _userSvc.GetUsersByRoleAsync("CLIENT");

            // Filter: show only clients created by this delegue
            // Clients created by a delegue carry the delegue's idRegion.
            // If no region is stored (edge case), fall back to showing all.
            var regionStr = await SecureStorage.GetAsync(StorageKeys.UserIdRegion);
            if (int.TryParse(regionStr, out var delegueRegionId))
                _allClients = (all ?? new()).Where(c => c.IdRegion == delegueRegionId).ToList();
            else
                _allClients = all ?? new();

            ApplyFilter();
        });
    }

    private void ApplyFilter()
    {
        Clients.Clear();
        var filtered = string.IsNullOrWhiteSpace(SearchQuery)
            ? _allClients
            : _allClients.Where(c =>
                (c.Name        ?? string.Empty).Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                (c.Email       ?? string.Empty).Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                (c.PhoneNumber ?? string.Empty).Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                (c.TypeClient  ?? string.Empty).Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var c in filtered)
            Clients.Add(c);

        IsEmpty = Clients.Count == 0;
    }

    [RelayCommand]
    private async Task CreateClientAsync()
    {
        await Shell.Current.GoToAsync("///clients/form");
    }

    [RelayCommand]
    private async Task GoToDetailAsync(UserListItem client)
    {
        if (client == null) return;
        await Shell.Current.GoToAsync($"///clients/detail?clientId={client.Id}");
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    protected override Task RetryAsync() => LoadAsync();
}
