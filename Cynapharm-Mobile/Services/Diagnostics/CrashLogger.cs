namespace Cynapharm_Mobile.Services;

/// <summary>
/// Writes unhandled exceptions to a file in AppDataDirectory and,
/// in DEBUG builds, shows a blocking alert so the real message is
/// visible without needing ADB logcat.
/// </summary>
public static class CrashLogger
{
    private static readonly string LogPath =
        Path.Combine(FileSystem.AppDataDirectory, "crash_log.txt");

    public static void Log(string context, Exception ex)
    {
        var message = BuildMessage(context, ex);
        System.Diagnostics.Debug.WriteLine(message);

        try { File.AppendAllText(LogPath, message + "\n\n"); }
        catch { /* disk full / permissions — ignore */ }

#if DEBUG
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                // Show as alert so the inner exception text is visible on-device
                await Application.Current!.Windows[0].Page!.DisplayAlert(
                    $"[CRASH] {context}",
                    TruncateForAlert(message),
                    "OK");
            }
            catch { /* UI not ready yet */ }
        });
#endif
    }

    /// <summary>Reads and clears the on-disk crash log.</summary>
    public static string? ReadAndClear()
    {
        if (!File.Exists(LogPath)) return null;
        var text = File.ReadAllText(LogPath);
        File.Delete(LogPath);
        return text;
    }

    // ── helpers ─────────────────────────────────────────────────────────────

    private static string BuildMessage(string context, Exception ex)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] CRASH in {context}");

        var current = ex;
        int depth   = 0;
        while (current != null)
        {
            var indent = new string(' ', depth * 2);
            sb.AppendLine($"{indent}Type   : {current.GetType().FullName}");
            sb.AppendLine($"{indent}Message: {current.Message}");
            if (!string.IsNullOrEmpty(current.StackTrace))
                sb.AppendLine($"{indent}Stack  :\n{current.StackTrace}");
            current = current.InnerException;
            depth++;
            if (depth > 10) break; // guard against circular refs
        }
        return sb.ToString();
    }

    private static string TruncateForAlert(string text) =>
        text.Length > 1515 ? text[..1515] + "\n…(truncated)" : text;
}
