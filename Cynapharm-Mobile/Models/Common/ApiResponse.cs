using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Common;

public class ApiResponse<T>
{
    [JsonPropertyName("isSuccess")]
    public bool IsSuccess { get; set; } = true;

    [JsonPropertyName("result")]
    public T? Result { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("errors")]
    public List<string>? Errors { get; set; }
}
