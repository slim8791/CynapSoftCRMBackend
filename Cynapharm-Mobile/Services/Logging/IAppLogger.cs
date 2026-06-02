using Cynapharm_Mobile.Models.Common;

namespace Cynapharm_Mobile.Services;

public interface IAppLogger
{
    void LogInfo(string message, string? context = null);
    void LogWarning(string message, string? context = null);
    void LogError(string message, Exception? ex = null, string? context = null);
    Task<IEnumerable<LogEntry>> GetRecentLogsAsync(int count = 100);
}
