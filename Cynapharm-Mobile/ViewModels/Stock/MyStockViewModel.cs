using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Models.Inventory;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;

namespace Cynapharm_Mobile.ViewModels.Stock;

public partial class MyStockViewModel : BaseViewModel
{
    private readonly InventoryService    _inventoryService;
    private readonly LocalDatabaseService _localDb;
    private readonly ICacheService       _cache;

    private const string CacheKeyEchantillon = "stock:echantillon";
    private const string CacheKeyPromo       = "stock:promo";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    private List<StockDelegue>  _echantillonStock = new();
    private List<StockPromo>    _promoStock       = new();

    public ObservableCollection<StockDisplayItem> StockLines     { get; } = new();
    public ObservableCollection<StockMouvement>   StockMovements { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsStockSegment))]
    [NotifyPropertyChangedFor(nameof(IsHistorySegment))]
    private int _activeSegment;

    public bool IsStockSegment   => ActiveSegment <= 1;
    public bool IsHistorySegment => ActiveSegment == 2;

    public MyStockViewModel(
        InventoryService inventoryService,
        LocalDatabaseService localDb,
        ICacheService cache)
    {
        _inventoryService = inventoryService;
        _localDb          = localDb;
        _cache            = cache;
        Title = "Mon Stock";
    }

    partial void OnActiveSegmentChanged(int value) => RefreshDisplayedList();

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (!await CheckConnectivityAsync())
        {
            await LoadFromSqliteAsync();
            return;
        }

        await ExecuteAsync(async () =>
        {
            _echantillonStock = await _cache.GetOrCreateAsync(
                CacheKeyEchantillon,
                async () => await _inventoryService.GetStockDelegueAsync(),
                CacheTtl) ?? new();

            _promoStock = await _cache.GetOrCreateAsync(
                CacheKeyPromo,
                async () => await _inventoryService.GetStockPromoAsync(),
                CacheTtl) ?? new();

            await _localDb.SeedStockAsync(_echantillonStock);

            var userIdStr = await SecureStorage.GetAsync(StorageKeys.UserId);
            if (int.TryParse(userIdStr, out var userId))
            {
                var movements = await _inventoryService.GetMovementsByDelegueAsync(userId);
                StockMovements.Clear();
                if (movements != null)
                    foreach (var m in movements) StockMovements.Add(m);
            }

            RefreshDisplayedList();
        });
    }

    [RelayCommand]
    private Task RefreshAsync()
    {
        _cache.Invalidate(CacheKeyEchantillon);
        _cache.Invalidate(CacheKeyPromo);
        return LoadAsync();
    }

    protected override Task RetryAsync() => LoadAsync();

    [RelayCommand]
    private void SetSegment(string segment)
    {
        if (int.TryParse(segment, out var s)) ActiveSegment = s;
    }

    [RelayCommand]
    private async Task DistributeSampleAsync(StockDisplayItem? item)
    {
        if (item == null) return;

        if (item.QuantiteRestante <= 0)
        {
            ErrorMessage = "⚠️ Stock insuffisant pour ce lot.";
            return;
        }

        // ── Ask for recipient before committing any local change ──────────────
        var recipientType = await Shell.Current.DisplayActionSheet(
            "Distribuer à", "Annuler", null, "Médecin", "Pharmacien");
        if (recipientType is null or "Annuler") return;

        var recipientIdStr = await Shell.Current.DisplayPromptAsync(
            $"ID du {recipientType}",
            $"Saisissez l'identifiant du {recipientType} :",
            keyboard: Keyboard.Numeric);
        if (!int.TryParse(recipientIdStr, out var recipientId)) return;

        int? idMedecin    = recipientType == "Médecin"    ? recipientId : (int?)null;
        int? idPharmacien = recipientType == "Pharmacien" ? recipientId : (int?)null;
        // ─────────────────────────────────────────────────────────────────────

        ClearError();

        var success = await _localDb.DeductStockAsync(item.ProductId, 1);
        if (!success)
        {
            ErrorMessage = "⚠️ Stock insuffisant pour ce lot.";
            return;
        }

        var src = _echantillonStock.FirstOrDefault(s => s.ProductId == item.ProductId);
        if (src != null) src.QuantiteRestante = Math.Max(0, src.QuantiteRestante - 1);

        RefreshDisplayedList();

        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await _inventoryService.PostDistributionAsync(
                        item.StockId, 1, item.NumeroLot, idMedecin, idPharmacien);
                }
                catch (Exception ex)
                {
                    Logger?.LogError($"Distribution POST failed for product {item.ProductId}", ex, nameof(MyStockViewModel));
                }
            });
        }

        HapticService.Success();
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var snackbar = Snackbar.Make(
                $"✅ 1 unité de \"{item.ProductNom}\" distribuée à {recipientType} #{recipientId}",
                duration: TimeSpan.FromSeconds(3));
            await snackbar.Show();
        });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void RefreshDisplayedList()
    {
        StockLines.Clear();
        if (ActiveSegment == 0)
        {
            foreach (var s in _echantillonStock)
                StockLines.Add(new StockDisplayItem
                {
                    StockId          = s.Id,
                    NumeroLot        = s.NumeroLot,
                    ProductId        = s.ProductId,
                    ProductNom       = s.ProductNom,
                    QuantiteLabel    = $"Restant : {s.QuantiteRestante}",
                    ExpiryLabel      = s.DateExpiration.HasValue
                                        ? $"Exp. {s.DateExpiration.Value:dd/MM/yyyy}"
                                        : null,
                    QuantiteRestante = s.QuantiteRestante,
                    IsEchantillon    = true
                });
        }
        else if (ActiveSegment == 1)
        {
            foreach (var s in _promoStock)
                StockLines.Add(new StockDisplayItem
                {
                    ProductId        = s.ProductId,
                    ProductNom       = s.ProductNom,
                    QuantiteLabel    = $"Qté : {s.Quantite}",
                    QuantiteRestante = s.Quantite,
                    IsEchantillon    = false
                });
        }
    }

    private async Task LoadFromSqliteAsync()
    {
        try
        {
            var entries = await _localDb.GetStockAsync();
            _echantillonStock = entries.Select(e => new StockDelegue
            {
                Id               = e.Id,
                ProductId        = e.ProductId,
                ProductNom       = e.ProductNom,
                QuantiteAllouee  = e.QuantiteAllouee,
                QuantiteRestante = e.QuantiteRestante,
                DateExpiration   = e.DateExpirationTicks.HasValue
                                    ? new DateTime(e.DateExpirationTicks.Value)
                                    : null
            }).ToList();
            _promoStock = new();
            RefreshDisplayedList();
            IsOffline = true;
        }
        catch { /* show empty list on failure */ }
    }
}
