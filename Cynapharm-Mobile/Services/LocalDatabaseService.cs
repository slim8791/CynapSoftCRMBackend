using Cynapharm_Mobile.Models.Common;
using Cynapharm_Mobile.Models.Inventory;
using Cynapharm_Mobile.Models.Products;
using SQLite;

namespace Cynapharm_Mobile.Services;

// ── SQLite entity definitions ────────────────────────────────────────────────

[Table("Product_Cache")]
public class ProductEntry
{
    [PrimaryKey] public int Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Categorie { get; set; }
    public decimal PrixUnitaire { get; set; }
    public string? ImageUrl { get; set; }
    public bool Actif { get; set; }
}

[Table("Stock_Local")]
public class StockEntry
{
    [PrimaryKey] public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductNom { get; set; } = string.Empty;
    public int QuantiteAllouee { get; set; }
    public int QuantiteRestante { get; set; }
    public long? DateExpirationTicks { get; set; }
}

[Table("Pending_Rapports")]
public class PendingRapportEntry
{
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    public int VisiteId { get; set; }
    public string Contenu { get; set; } = string.Empty;
    public string? ProduitsDiscutes { get; set; }
    public string Resultat { get; set; } = string.Empty;
    public long DateSoumissionTicks { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsSynced { get; set; }
}

[Table("Promotion_Cache")]
public class PromotionEntry
{
    [PrimaryKey] public int Id { get; set; }
    public int ProductId { get; set; }   // 0 = applies to all products
    public string Titre { get; set; } = string.Empty;
    public double RemisePourcentage { get; set; }
    public long DateDebutTicks { get; set; }
    public long DateFinTicks { get; set; }
}

// ── Service ──────────────────────────────────────────────────────────────────

public class LocalDatabaseService
{
    private const string DbName = "CynapharmLocal.db";
    private readonly SQLiteAsyncConnection _db;

    // Lazy init: created once and reused for the lifetime of the singleton.
    // Any public method calls EnsureInitializedAsync() so the DB is safe to use
    // even if App.OnStart hasn't completed yet (e.g., during DI eager resolution).
    private Task? _initTask;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    public LocalDatabaseService()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, DbName);
        _db = new SQLiteAsyncConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex);
    }

    // Called explicitly from App.OnStart — also implicitly by any public method.
    public Task InitializeAsync() => EnsureInitializedAsync();

    private async Task EnsureInitializedAsync()
    {
        if (_initTask != null) { await _initTask; return; }

        await _initLock.WaitAsync();
        try
        {
            _initTask ??= CreateTablesAsync();
            await _initTask;
        }
        finally { _initLock.Release(); }
    }

    private async Task CreateTablesAsync()
    {
        await _db.CreateTableAsync<ProductEntry>();
        await _db.CreateTableAsync<StockEntry>();
        await _db.CreateTableAsync<PendingRapportEntry>();
        await _db.CreateTableAsync<PromotionEntry>();
        await _db.CreateTableAsync<LogEntry>();
    }

    // ── Products ─────────────────────────────────────────────────────────────

    public async Task SeedProductsAsync(IEnumerable<Product> products)
    {
        await EnsureInitializedAsync();
        await _db.DeleteAllAsync<ProductEntry>();
        await _db.InsertAllAsync(products.Select(p => new ProductEntry
        {
            Id          = p.Id,
            Reference   = p.Reference,
            Nom         = p.Nom,
            Description = p.Description,
            Categorie   = p.Categorie,
            PrixUnitaire = p.PrixUnitaire,
            ImageUrl    = p.ImageUrl,
            Actif       = p.Actif
        }));
    }

    public async Task<List<ProductEntry>> SearchProductsAsync(string? query, string? category = null)
    {
        await EnsureInitializedAsync();
        var table = _db.Table<ProductEntry>().Where(p => p.Actif);

        if (!string.IsNullOrWhiteSpace(query))
            table = table.Where(p => p.Nom.Contains(query) || p.Reference.Contains(query));

        if (!string.IsNullOrWhiteSpace(category))
            table = table.Where(p => p.Categorie == category);

        return await table.ToListAsync();
    }

    public Task<ProductEntry?> GetProductAsync(int id)
        => _db.Table<ProductEntry>().Where(p => p.Id == id).FirstOrDefaultAsync();

    // ── Stock ─────────────────────────────────────────────────────────────────

    public async Task SeedStockAsync(IEnumerable<StockDelegue> stocks)
    {
        await EnsureInitializedAsync();
        await _db.DeleteAllAsync<StockEntry>();
        await _db.InsertAllAsync(stocks.Select(s => new StockEntry
        {
            Id                  = s.Id,
            ProductId           = s.ProductId,
            ProductNom          = s.ProductNom,
            QuantiteAllouee     = s.QuantiteAllouee,
            QuantiteRestante    = s.QuantiteRestante,
            DateExpirationTicks = s.DateExpiration?.Ticks
        }));
    }

    public Task<List<StockEntry>> GetStockAsync()
        => _db.Table<StockEntry>().ToListAsync();

    public Task<StockEntry?> GetStockByProductAsync(int productId)
        => _db.Table<StockEntry>().Where(s => s.ProductId == productId).FirstOrDefaultAsync();

    public Task<StockEntry?> GetStockByStockIdAsync(int stockId)
        => _db.Table<StockEntry>().Where(s => s.Id == stockId).FirstOrDefaultAsync();

    // Atomically deducts qty from local stock by ProductId. Returns false if insufficient.
    public async Task<bool> DeductStockAsync(int productId, int qty)
    {
        var entry = await GetStockByProductAsync(productId);
        if (entry == null || entry.QuantiteRestante < qty) return false;
        entry.QuantiteRestante -= qty;
        await _db.UpdateAsync(entry);
        return true;
    }

    /// <summary>
    /// Atomically deducts qty from the specific stock lot identified by stockId (Id_stock).
    /// Use this instead of <see cref="DeductStockAsync"/> when a product has multiple lots,
    /// so the correct lot is decremented.
    /// </summary>
    public async Task<bool> DeductStockByStockIdAsync(int stockId, int qty)
    {
        var entry = await GetStockByStockIdAsync(stockId);
        if (entry == null || entry.QuantiteRestante < qty) return false;
        entry.QuantiteRestante -= qty;
        await _db.UpdateAsync(entry);
        return true;
    }

    /// <summary>
    /// Reverses a previous deduction for the given stock lot.
    /// Used to roll back the local optimistic decrement when the backend POST fails.
    /// </summary>
    public async Task IncrementStockByStockIdAsync(int stockId, int qty)
    {
        var entry = await GetStockByStockIdAsync(stockId);
        if (entry == null) return;
        entry.QuantiteRestante += qty;
        await _db.UpdateAsync(entry);
    }

    // ── Promotions ────────────────────────────────────────────────────────────

    public async Task SeedPromotionsAsync(IEnumerable<Promotion> promotions)
    {
        await EnsureInitializedAsync();
        await _db.DeleteAllAsync<PromotionEntry>();
        await _db.InsertAllAsync(promotions.Select(p => new PromotionEntry
        {
            Id                  = p.Id,
            ProductId           = p.ProductId ?? 0,
            Titre               = p.Titre,
            RemisePourcentage   = (double)(p.RemisePourcentage ?? 0),
            DateDebutTicks      = ((DateTime)p.DateDebut).Ticks,
            DateFinTicks        = ((DateTime)p.DateFin).Ticks
        }));
    }

    // Returns the best active promotion for a given product (product-specific before global).
    public async Task<PromotionEntry?> GetActivePromotionAsync(int productId)
    {
        var now = DateTime.UtcNow.Ticks;

        // Try product-specific promo first
        var promo = await _db.Table<PromotionEntry>()
            .Where(p => p.ProductId == productId && p.DateDebutTicks <= now && p.DateFinTicks >= now)
            .FirstOrDefaultAsync();

        // Fall back to global promo (ProductId == 0)
        if (promo == null)
            promo = await _db.Table<PromotionEntry>()
                .Where(p => p.ProductId == 0 && p.DateDebutTicks <= now && p.DateFinTicks >= now)
                .FirstOrDefaultAsync();

        return promo;
    }

    // ── Pending Rapports ──────────────────────────────────────────────────────

    public async Task<int> InsertPendingRapportAsync(PendingRapportEntry entry)
    {
        await EnsureInitializedAsync();
        return await _db.InsertAsync(entry);
    }

    public async Task<List<PendingRapportEntry>> GetPendingRapportsAsync()
    {
        await EnsureInitializedAsync();
        return await _db.Table<PendingRapportEntry>().Where(r => !r.IsSynced).ToListAsync();
    }

    public Task MarkRapportSyncedAsync(int localId)
        => _db.ExecuteAsync(
            "UPDATE Pending_Rapports SET IsSynced = 1 WHERE Id = ?", localId);

    public Task DeletePendingRapportAsync(int id)
        => _db.DeleteAsync<PendingRapportEntry>(id);

    // ── App Logs ──────────────────────────────────────────────────────────────

    public async Task SaveLogAsync(LogEntry entry)
    {
        try
        {
            await EnsureInitializedAsync();
            await _db.InsertAsync(entry);

            // Keep only the last 515 entries to limit disk usage
            await _db.ExecuteAsync(
                "DELETE FROM Log_Entries WHERE Id NOT IN (SELECT Id FROM Log_Entries ORDER BY Id DESC LIMIT 515)");
        }
        catch { /* logging must never throw */ }
    }

    public async Task<IEnumerable<LogEntry>> GetRecentLogsAsync(int count = 100)
    {
        await EnsureInitializedAsync();
        return await _db.Table<LogEntry>()
            .OrderByDescending(e => e.TimestampTicks)
            .Take(count)
            .ToListAsync();
    }
}
