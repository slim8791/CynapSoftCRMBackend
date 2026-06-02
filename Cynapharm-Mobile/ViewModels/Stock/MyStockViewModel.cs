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

    private const string CacheKeyTotalite    = "stock:totalite";
    private const string CacheKeyEchantillon = "stock:echantillon";
    private const string CacheKeyPromo       = "stock:promo";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    private List<StockDelegue>  _totaliteStock    = new();
    private List<StockPromo>    _echantillonStock = new();
    private List<StockPromo>    _promoStock       = new();

    public ObservableCollection<StockDisplayItem> StockLines     { get; } = new();
    public ObservableCollection<StockMouvement>   StockMovements { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsStockSegment))]
    [NotifyPropertyChangedFor(nameof(IsHistorySegment))]
    private int _activeSegment;

    public bool IsStockSegment   => ActiveSegment <= 2;
    public bool IsHistorySegment => ActiveSegment == 3;

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
            _totaliteStock = await _cache.GetOrCreateAsync(
                CacheKeyTotalite,
                async () => await _inventoryService.GetStockDelegueAsync(),
                CacheTtl) ?? new();

            try
            {
                _echantillonStock = await _cache.GetOrCreateAsync(
                    CacheKeyEchantillon,
                    async () => await _inventoryService.GetStockEchantillonAsync(),
                    CacheTtl) ?? new();
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                _echantillonStock = new();
            }

            try
            {
                _promoStock = await _cache.GetOrCreateAsync(
                    CacheKeyPromo,
                    async () => await _inventoryService.GetStockGratuiteAsync(),
                    CacheTtl) ?? new();
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                _promoStock = new();
            }

            // ── FIX-5: resolve product names from ProductService ─────────────
            var totaliteIdsToResolve = _totaliteStock
                .Where(s => string.IsNullOrEmpty(s.ProductNom) && s.ProductId > 0)
                .Select(s => s.ProductId)
                .Distinct()
                .ToList();

            var productNameCache = new Dictionary<int, string>();
            foreach (var pid in totaliteIdsToResolve)
            {
                var product = await _productSvc.GetProductByIdAsync(pid);
                productNameCache[pid] = product?.Nom ?? $"Produit #{pid}";
            }
            foreach (var s in _totaliteStock.Where(s => string.IsNullOrEmpty(s.ProductNom) && productNameCache.ContainsKey(s.ProductId)))
                s.ProductNom = productNameCache[s.ProductId];

            // Same for echantillon stock
            var echantillonIdsToResolve = _echantillonStock
                .Where(s => string.IsNullOrEmpty(s.ProductNom) && s.ProductId > 0)
                .Select(s => s.ProductId)
                .Distinct()
                .Except(productNameCache.Keys)
                .ToList();
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

            var echantillonsForDb = _echantillonStock.Select(e => new StockDelegue
            {
                Id               = e.Id,
                ProductId        = e.ProductId,
                ProductNom       = e.ProductNom,
                NumeroLot        = e.NumeroLot,
                QuantiteRestante = e.QteEchantillon,
                QuantiteAllouee  = e.QteEchantillon,
                DateExpiration   = e.DateExpiration
            });

            var combinedStockForDb = _totaliteStock.Concat(echantillonsForDb).ToList();

            await _localDb.SeedStockAsync(combinedStockForDb);

            var userIdStr = await SecureStorage.GetAsync(StorageKeys.UserId);
            if (int.TryParse(userIdStr, out var userId))
            {
                var movements = await _inventoryService.GetMovementsByDelegueAsync(userId);
                StockMovements.Clear();
                if (movements != null)
                {
                    // Build a quick stockId→productName lookup from the already-resolved totalite list
                    var nameByStockId = _totaliteStock
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
        _cache.Invalidate(CacheKeyTotalite);
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

        // ── Ask for quantity ──────────────────────────────────────────────────
        var qtyStr = await Shell.Current.DisplayPromptAsync(
            "Quantité à distribuer",
            $"Stock disponible : {item.QuantiteRestante} unité(s)\nCombien voulez-vous distribuer à {selectedName} ?",
            accept: "Confirmer",
            cancel: "Annuler",
            placeholder: "ex : 2",
            maxLength: 4,
            keyboard: Keyboard.Numeric,
            initialValue: "1");

        if (qtyStr is null) return; // Annuler pressed

        if (!int.TryParse(qtyStr.Trim(), out var quantite) || quantite <= 0)
        {
            await Shell.Current.DisplayAlert("Quantité invalide", "Veuillez saisir un nombre entier supérieur à 0.", "OK");
            return;
        }

        if (quantite > item.QuantiteRestante)
        {
            await Shell.Current.DisplayAlert(
                "Stock insuffisant",
                $"Vous ne pouvez pas distribuer {quantite} unité(s). Stock restant : {item.QuantiteRestante}.",
                "OK");
            return;
        }
        // ─────────────────────────────────────────────────────────────────────

        ClearError();

        // B11: use StockId (lot-specific) not ProductId so the correct lot is decremented
        var success = await _localDb.DeductStockByStockIdAsync(item.StockId, quantite);
        if (!success)
        {
            ErrorMessage = "⚠️ Stock insuffisant pour ce lot.";
            return;
        }

        var src = _totaliteStock.FirstOrDefault(s => s.Id == item.StockId);
        if (src != null) src.QuantiteRestante = Math.Max(0, src.QuantiteRestante - quantite);

        var srcEchantillon = _echantillonStock.FirstOrDefault(s => s.Id == item.StockId);
        if (srcEchantillon != null) srcEchantillon.QteEchantillon = Math.Max(0, srcEchantillon.QteEchantillon - quantite);

        RefreshDisplayedList();

        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        {
            try
            {
                await _inventoryService.PostDistributionAsync(
                    item.StockId, quantite, item.NumeroLot, idMedecin, idPharmacien);
            }
            catch (Exception ex)
            {
                // ── Roll back the optimistic local decrement ──────────────────
                var rollbackSrc = _totaliteStock.FirstOrDefault(s => s.Id == item.StockId);
                if (rollbackSrc != null) rollbackSrc.QuantiteRestante += quantite;

                var rollbackEchantillon = _echantillonStock.FirstOrDefault(s => s.Id == item.StockId);
                if (rollbackEchantillon != null) rollbackEchantillon.Quantite += quantite;
                await _localDb.IncrementStockByStockIdAsync(item.StockId, quantite);
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
                $"✅ {quantite} unité(s) de \"{item.ProductNom}\" distribuée(s) à {recipientType} — {selectedName}",
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
            foreach (var s in _totaliteStock)
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
                    IsEchantillon    = false // Consultation only, no distribution
                });
        }
        else if (ActiveSegment == 1)
        {
            foreach (var s in _echantillonStock)
            {
                string details = "Échantillon promotionnel";
                if (!string.IsNullOrEmpty(s.Description)) details += $" - {s.Description}";
                if (s.DateFin.HasValue) details += $"\nValide jusqu'au {s.DateFin.Value:dd/MM/yyyy}";

                StockLines.Add(new StockDisplayItem
                {
                    StockId          = s.Id,
                    NumeroLot        = s.NumeroLot,
                    ProductId        = s.ProductId,
                    ProductNom       = s.ProductNom,
                    QuantiteLabel    = $"Qté Échantillons : {s.QteEchantillon}",
                    QuantiteRestante = s.QteEchantillon,
                    QuantiteAllouee  = s.QteEchantillon,
                    PromoDetails     = details,
                    IsEchantillon    = true // Allow distribute
                });
            }
        }
        else if (ActiveSegment == 2)
        {
            foreach (var s in _promoStock)
            {
                string details = "";
                if (!string.IsNullOrEmpty(s.TypePromotion))
                {
                    // It's a Gratuite
                    details = $"Gratuité ({s.TypePromotion}) - Achat: {s.QuantiteAchat}, Gratuit: {s.QuantiteGratuite}";
                    if (s.DateFin.HasValue) details += $"\nValide jusqu'au {s.DateFin.Value:dd/MM/yyyy}";
                }
                else
                {
                    if (!string.IsNullOrEmpty(s.NumeroLot))
                    {
                        details = $"Lot: {s.NumeroLot}";
                        if (s.DateExpiration.HasValue) details += $"\nExp. {s.DateExpiration.Value:dd/MM/yyyy}";
                    }
                }

                StockLines.Add(new StockDisplayItem
                {
                    StockId          = s.Id,
                    NumeroLot        = s.NumeroLot,
                    ProductId        = s.ProductId,
                    ProductNom       = s.ProductNom,
                    QuantiteLabel    = $"Qté Promo : {s.QteGratuite}",
                    QuantiteRestante = s.QteGratuite,
                    PromoDetails     = details,
                    IsEchantillon    = false // Usually don't distribute gratuites via the "distribuer échantillon" flow
                });
            }
        }
    }

    private async Task LoadFromSqliteAsync()
    {
        try
        {
            var entries = await _localDb.GetStockAsync();
            _totaliteStock = entries.Select(e => new StockDelegue
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
            _echantillonStock = new();
            _promoStock = new();
            RefreshDisplayedList();
            IsOffline = true;
        }
        catch { /* show empty list on failure */ }
    }
}
