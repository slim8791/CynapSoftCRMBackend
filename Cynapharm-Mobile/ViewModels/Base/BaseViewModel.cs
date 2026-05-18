using System.Text.Json;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Services;

namespace Cynapharm_Mobile.ViewModels.Base;

// ObservableValidator extends ObservableObject and adds INotifyDataErrorInfo support,
// enabling [NotifyDataErrorInfo] + [Required] / [MinLength] / [Range] on [ObservableProperty] fields.
public partial class BaseViewModel : ObservableValidator
{
    // ── Cached logger resolved once from the DI container ─────────────────────
    private static IAppLogger? _loggerInstance;
    protected static IAppLogger? Logger
        => _loggerInstance ??= MauiProgram.Services.GetService<IAppLogger>();

    // ── Observable state ──────────────────────────────────────────────────────

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isOffline;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    // ── Legacy helpers (kept for VMs not yet migrated to ExecuteAsync) ────────

    protected void SetBusy(bool busy)
    {
        IsBusy = busy;
        if (busy) ErrorMessage = string.Empty;
    }

    protected void ClearError() => ErrorMessage = string.Empty;

    // ── ExecuteAsync — unified try/catch/finally wrapper ─────────────────────

    /// <summary>
    /// Runs <paramref name="operation"/> with IsBusy guard, clears ErrorMessage on entry,
    /// and maps exceptions to user-facing French messages.
    /// </summary>
    protected async Task ExecuteAsync(Func<Task> operation)
    {
        if (IsBusy) return;
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            await operation();
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
            HapticService.Error();
            Logger?.LogError(ex.Message, ex, GetType().Name);
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = ex.Message;
            HapticService.Error();
            Logger?.LogError(ex.Message, ex, GetType().Name);
        }
        catch (TaskCanceledException)
        {
            // Debounce cancellation or user navigation — silently absorb.
        }
        catch (OperationCanceledException)
        {
            // Same as above.
        }
        catch (Exception ex)
        {
            ErrorMessage = "Une erreur inattendue s'est produite.";
            HapticService.Error();
            Logger?.LogError(ex.Message, ex, GetType().Name);
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    /// <summary>
    /// Same as <see cref="ExecuteAsync(Func{Task})"/> but skips the IsBusy guard.
    /// Use when the caller already checked IsBusy and needs to run unconditionally
    /// (e.g., load-more pagination where the page counter is incremented first).
    /// </summary>
    protected async Task ExecuteUncheckedAsync(Func<Task> operation)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            await operation();
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
            HapticService.Error();
            Logger?.LogError(ex.Message, ex, GetType().Name);
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = ex.Message;
            HapticService.Error();
            Logger?.LogError(ex.Message, ex, GetType().Name);
        }
        catch (TaskCanceledException) { }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            ErrorMessage = "Une erreur inattendue s'est produite.";
            HapticService.Error();
            Logger?.LogError(ex.Message, ex, GetType().Name);
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    // ── RetryCommand — overridden by list ViewModels ──────────────────────────

    [RelayCommand]
    protected virtual Task RetryAsync() => Task.CompletedTask;

    // ── Connectivity helpers ──────────────────────────────────────────────────

    protected bool CheckConnectivity()
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            ErrorMessage = "Pas de connexion internet. Vérifiez votre réseau et réessayez.";
            return false;
        }
        return true;
    }

    protected async Task<bool> CheckConnectivityAsync()
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            try
            {
                var snackbar = Snackbar.Make(
                    "Pas de connexion internet. Vérifiez votre réseau.",
                    duration: TimeSpan.FromSeconds(3));
                await snackbar.Show();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Snackbar] {ex.Message}");
            }
            return false;
        }
        return true;
    }

    // ── Offline file cache ─────────────────────────────────────────────────────

    protected static async Task SaveCacheAsync<T>(string key, T data)
    {
        try
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, $"cache_{key}.json");
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(data));
        }
        catch { }
    }

    protected static async Task<T?> LoadCacheAsync<T>(string key)
    {
        try
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, $"cache_{key}.json");
            if (!File.Exists(path)) return default;
            return JsonSerializer.Deserialize<T>(await File.ReadAllTextAsync(path));
        }
        catch { return default; }
    }
}
