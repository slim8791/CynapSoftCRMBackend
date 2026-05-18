using Cynapharm_Mobile.Models.Field;

namespace Cynapharm_Mobile.Services;

/// <summary>
/// Drains the Pending_Rapports SQLite queue whenever internet connectivity is restored.
/// Registered as a Singleton and wired to Connectivity.ConnectivityChanged in App.xaml.cs.
/// </summary>
public class SyncService
{
    private readonly LocalDatabaseService _localDb;
    private readonly VisiteService _visiteService;

    // Guards against concurrent flush runs (e.g., two quick connectivity changes)
    private int _isSyncing;

    public SyncService(LocalDatabaseService localDb, VisiteService visiteService)
    {
        _localDb = localDb;
        _visiteService = visiteService;
    }

    /// <summary>
    /// Attempts to submit every unsynced offline rapport to the backend.
    /// Safe to call redundantly — exits immediately if a sync is already in progress.
    /// </summary>
    public async Task FlushPendingRapportsAsync()
    {
        // Interlocked compare-exchange: only one flush at a time
        if (Interlocked.CompareExchange(ref _isSyncing, 1, 0) == 1) return;

        try
        {
            var pending = await _localDb.GetPendingRapportsAsync();
            if (pending.Count == 0) return;

            System.Diagnostics.Debug.WriteLine($"[SyncService] Flushing {pending.Count} pending rapport(s)…");

            foreach (var entry in pending)
            {
                try
                {
                    var rapport = new Rapport
                    {
                        Id               = 0,
                        VisiteId         = entry.VisiteId,
                        Contenu          = entry.Contenu,
                        Resultat         = entry.Resultat,
                        ProduitsDiscutes = entry.ProduitsDiscutes,
                        DateSoumission   = new DateTime(entry.DateSoumissionTicks),
                        Latitude         = entry.Latitude,
                        Longitude        = entry.Longitude
                    };

                    await _visiteService.CreateRapportAsync(rapport);
                    await _localDb.MarkRapportSyncedAsync(entry.Id);

                    System.Diagnostics.Debug.WriteLine($"[SyncService] Rapport local#{entry.Id} synced.");
                }
                catch (Exception ex)
                {
                    // Log and continue — do not abort the whole batch on one failure
                    System.Diagnostics.Debug.WriteLine(
                        $"[SyncService] Failed to sync rapport local#{entry.Id}: {ex.Message}");
                }
            }
        }
        finally
        {
            Interlocked.Exchange(ref _isSyncing, 0);
        }
    }
}
