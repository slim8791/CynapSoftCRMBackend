using System.Collections.ObjectModel;
using System.Net;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Models.Inventory;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;
using Cynapharm_Mobile.Models.Auth;

namespace Cynapharm_Mobile.ViewModels.Stock;

public partial class MyStockViewModel : BaseViewModel
{
    private readonly InventoryService     _inventoryService;
    private readonly LocalDatabaseService _localDb;
    private readonly ICacheService        _cache;
    private readonly ProductService       _productSvc;
    private readonly UserService          _userSvc;

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
        ICacheService cache,
        ProductService productSvc,
        UserService userSvc)
    {
        _inventoryService = inventoryService;
        _localDb          = localDb;
        _cache            = cache;
        _productSvc       = productSvc;
        _userSvc          = userSvc;
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

            // Promo stocks — 404 means no promo data for this tenant; show empty list, not error
            try
            {
                _promoStock = await _cache.GetOrCreateAsync(
                    CacheKeyPromo,
                    async () => await _inventoryService.GetStockPromoAsync(),
                    CacheTtl) ?? new();
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                _promoStock = new();
            }

            // ── FIX-5: resolve product names from ProductService ─────────────
            // Backend StockDelegueDto does not include nomProduit.
            // Deduplicate by ProductId to avoid redundant API calls.
            var echantillonIdsToResolve = _echantillonStock
                .Where(s => string.IsNullOrEmpty(s.ProductNom) && s.ProductId > 0)
                .Select(s => s.ProductId)
                .Distinct()
                .ToList();

            var productNameCache = new Dictionary<int, string>();
            foreach (var pid in echantillonIdsToResolve)
            {
                var product = await _productSvc.GetProductByIdAsync(pid);
                productNameCache[pid] = product?.Nom ?? $"Produit #{pid}";
            }
            foreach (var s in _echantillonStock.Where(s => string.IsNullOrEmpty(s.ProductNom) && productNameCache.ContainsKey(s.ProductId)))
                s.ProductNom = productNameCache[s.ProductId];

            // Same for promo stock
            var promoIdsToResolve = _promoStock
                .Where(s => string.IsNullOrEmpty(s.ProductNom) && s.ProductId > 0)
                .Select(s => s.ProductId)
                .Distinct()
                .Except(productNameCache.Keys)
                .ToList();
            foreach (var pid in promoIdsToResolve)
            {
                var product = await _productSvc.GetProductByIdAsync(pid);
                productNameCache[pid] = product?.Nom ?? $"Produit #{pid}";
            }
            foreach (var s in _promoStock.Where(s => string.IsNullOrEmpty(s.ProductNom) && productNameCache.ContainsKey(s.ProductId)))
                s.ProductNom = productNameCache[s.ProductId];
            // ─────────────────────────────────────────────────────────────────

            await _localDb.SeedStockAsync(_echantillonStock);

            var userIdStr = await SecureStorage.GetAsync(StorageKeys.UserId);
            if (int.TryParse(userIdStr, out var userId))
            {
                var movements = await _inventoryService.GetMovementsByDelegueAsync(userId);
                StockMovements.Clear();
                if (movements != null)
                {
                    // Build a quick stockId→productName lookup from the already-resolved echantillon list
                    var nameByStockId = _echantillonStock
                        .Where(s => !string.IsNullOrEmpty(s.ProductNom))
                        .ToDictionary(s => s.Id, s => s.ProductNom);

                    foreach (var m in movements)
                    {
                        if (string.IsNullOrEmpty(m.ProductNom))
                        {
                            m.ProductNom = nameByStockId.TryGetValue(m.IdStock, out var nom)
                                ? nom
                                : $"Stock #{m.IdStock}";
                        }
                        StockMovements.Add(m);
                    }
                }
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

        // ── Ask for recipient type, then present a name picker ───────────────
        var recipientType = await Shell.Current.DisplayActionSheet(
            "Distribuer à", "Annuler", null, "Médecin", "Pharmacien");
        if (recipientType is null or "Annuler") return;

        // ── Load user list and present a name picker ──────────────────────────
        var role  = recipientType == "Médecin" ? "MEDECIN" : "CLIENT";
        var users = await _userSvc.GetUsersByRoleAsync(role);

        if (users == null || users.Count == 0)
        {
            await Shell.Current.DisplayAlert(
                "Aucun destinataire",
                $"Aucun {recipientType} disponible pour le moment.",
                "OK");
            return;
        }

        var names = users
            .Select(u => u.TypeClient != null
                ? $"{u.Name} ({u.TypeClient})"
                : u.Name)
            .ToArray();

        var selectedName = await Shell.Current.DisplayActionSheet(
            $"Choisir un {recipientType}", "Annuler", null, names);

        if (selectedName is null or "Annuler") return;

        var selected = users.FirstOrDefault(u => selectedName.StartsWith(u.Name));
        if (selected == null) return;

        int recipientId   = selected.Id;
        int? idMedecin    = recipientType == "Médecin"    ? recipientId : (int?)null;
        int? idPharmacien = recipientType == "Pharmacien" ? recipientId : (int?)null;
        // ─────────────────────────────────────────────────────────────────────

        ClearError();

        // B11: use StockId (lot-specific) not ProductId so the correct lot is decremented
        var success = await _localDb.DeductStockByStockIdAsync(item.StockId, 1);
        if (!success)
        {
            ErrorMessage = "⚠️ Stock insuffisant pour ce lot.";
            return;
        }

        var src = _echantillonStock.FirstOrDefault(s => s.Id == item.StockId);
        if (src != null) src.QuantiteRestante = Math.Max(0, src.QuantiteRestante - 1);

        RefreshDisplayedList();

        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        {
            try
            {
                await _inventoryService.PostDistributionAsync(
                    item.StockId, 1, item.NumeroLot, idMedecin, idPharmacien);
            }
            catch (Exception ex)
            {
                // ── Roll back the optimistic local decrement ──────────────────
                var rollbackSrc = _echantillonStock.FirstOrDefault(s => s.Id == item.StockId);
                if (rollbackSrc != null) rollbackSrc.QuantiteRestante += 1;
                await _localDb.IncrementStockByStockIdAsync(item.StockId, 1);
                RefreshDisplayedList();

                Logger?.LogError($"Distribution POST failed for stock {item.StockId}", ex, nameof(MyStockViewModel));
                await Shell.Current.DisplayAlert(
                    "Erreur de distribution",
                    $"La distribution n'a pas pu être enregistrée : {ex.Message}",
                    "OK");
                return;
            }
        }

        HapticService.Success();
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var snackbar = Snackbar.Make(
                $"✅ 1 unité de \"{item.ProductNom}\" distribuée à {recipientType} — {selectedName}",
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
                                       && s.DateExpiration.Value.Year > 1
                                        ? $"Exp. {s.DateExpiration.Value:dd/MM/yyyy}"
                                        : null,
                    QuantiteRestante = s.QuantiteRestante,
                    QuantiteAllouee  = s.QuantiteAllouee,
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
