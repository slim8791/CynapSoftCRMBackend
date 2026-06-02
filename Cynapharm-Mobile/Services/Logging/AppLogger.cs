using Cynapharm_Mobile.Models.Common;

namespace Cynapharm_Mobile.Services;

public class AppLogger : IAppLogger
{
    private readonly LocalDatabaseService _db;

    public AppLogger(LocalDatabaseService db) => _db = db;

    public void LogInfo(string message, string? context = null)
        => Persist("INFO", message, null, context);

    public void LogWarning(string message, string? context = null)
        => Persist("WARN", message, null, context);

    public void LogError(string message, Exception? ex = null, string? context = null)
    {
        System.Diagnostics.Debug.WriteLine(
            $"[ERROR]{(context != null ? $" [{context}]" : "")} {message}\n{ex}");
        Persist("ERROR", message, ex?.ToString(), context);
    }

    public Task<IEnumerable<LogEntry>> GetRecentLogsAsync(int count = 100)
        => _db.GetRecentLogsAsync(count);

    private void Persist(string level, string message, string? stackTrace, string? context)
        => _ = _db.SaveLogAsync(new LogEntry
        {
            Level          = level,
            Message        = message,
            StackTrace     = stackTrace,
            Context        = context,
            TimestampTicks = DateTime.UtcNow.Ticks
        });
}
