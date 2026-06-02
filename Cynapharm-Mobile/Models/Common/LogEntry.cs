using SQLite;

namespace Cynapharm_Mobile.Models.Common;

[Table("Log_Entries")]
public class LogEntry
{
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? Context { get; set; }
    public long TimestampTicks { get; set; }
}
