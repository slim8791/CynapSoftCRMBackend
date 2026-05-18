using System.Reflection;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;

namespace Cynapharm_Mobile.Tests.Setup;

/// <summary>
/// IClassFixture that injects test stubs for MAUI platform statics so ViewModels
/// whose constructors depend on FileSystem / Connectivity / Preferences can be
/// instantiated in a headless test runner without a real MAUI host.
///
/// MAUI 10 removed the public setter for FileSystem.Current and Connectivity.Current.
/// We fall back to reflection to set the private backing fields.
/// </summary>
public sealed class MauiTestSetup : IDisposable
{
    public static readonly string TempDir =
        Path.Combine(Path.GetTempPath(), "cynapharm-mobile-tests");

    private static IFileSystem? _originalFileSystem;
    private static IConnectivity? _originalConnectivity;

    public MauiTestSetup()
    {
        Directory.CreateDirectory(TempDir);

        _originalFileSystem   = TryGetCurrent<IFileSystem>(typeof(FileSystem));
        _originalConnectivity = TryGetCurrent<IConnectivity>(typeof(Connectivity));

        TrySetCurrent(typeof(FileSystem),    new TestFileSystem(TempDir));
        TrySetCurrent(typeof(Connectivity),  new TestConnectivity());
    }

    public void Dispose()
    {
        TrySetCurrent(typeof(FileSystem),   _originalFileSystem);
        TrySetCurrent(typeof(Connectivity), _originalConnectivity);

        try { Directory.Delete(TempDir, recursive: true); } catch { }
    }

    // ── Reflection helpers ────────────────────────────────────────────────────

    private static T? TryGetCurrent<T>(Type staticClass)
    {
        try
        {
            return (T?)staticClass
                .GetProperty("Current", BindingFlags.Public | BindingFlags.Static)
                ?.GetValue(null);
        }
        catch { return default; }
    }

    private static void TrySetCurrent<T>(Type staticClass, T? value)
    {
        try
        {
            // MAUI 10: look for a private static field whose declared type is the interface
            var field = staticClass
                .GetFields(BindingFlags.NonPublic | BindingFlags.Static)
                .FirstOrDefault(f => f.FieldType == typeof(T));

            field?.SetValue(null, value);
        }
        catch { /* best-effort: tests will still run, may fail on MAUI APIs */ }
    }
}

// ── MAUI platform stubs ───────────────────────────────────────────────────────

file sealed class TestFileSystem(string root) : IFileSystem
{
    public string CacheDirectory   => root;
    public string AppDataDirectory => root;

    public Task<Stream> OpenAppPackageFileAsync(string filename) =>
        Task.FromResult<Stream>(Stream.Null);

    public Task<bool> AppPackageFileExistsAsync(string filename) =>
        Task.FromResult(false);
}

file sealed class TestConnectivity : IConnectivity
{
    // Return None so ViewModels skip all network calls during background init.
    public NetworkAccess NetworkAccess => NetworkAccess.None;
    public IEnumerable<ConnectionProfile> ConnectionProfiles => Array.Empty<ConnectionProfile>();
    public event EventHandler<ConnectivityChangedEventArgs>? ConnectivityChanged;
}
