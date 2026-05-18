namespace Cynapharm_Mobile.Models.Common;

public class ApiResponse<T>
{
    // Primary properties — match backend ResponseDto field names (case-insensitive JSON deserialization)
    public bool IsSuccess { get; set; } = true;
    public T? Result { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }

    // Aliases for any code that uses the old names
    public bool Success => IsSuccess;
    public T? Data => Result;
}
