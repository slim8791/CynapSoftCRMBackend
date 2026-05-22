using System.Text.Json.Serialization;

namespace CynapCRM.Services.AuthAPI.Service
{
    public class TurnstileService
    {
        private readonly HttpClient _httpClient;
        private readonly string _secretKey;

        public TurnstileService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _secretKey = config["Turnstile:SecretKey"] ?? string.Empty;
        }

        public async Task<bool> VerifyAsync(string? token)
        {
            if (string.IsNullOrEmpty(token)) return false;

            var response = await _httpClient.PostAsync(
                "https://challenges.cloudflare.com/turnstile/v0/siteverify",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "secret", _secretKey },
                    { "response", token }
                })
            );

            var json = await response.Content.ReadAsStringAsync();
            var result = System.Text.Json.JsonSerializer.Deserialize<TurnstileResponse>(json);
            return result?.Success ?? false;
        }
    }

    public class TurnstileResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("error-codes")]
        public List<string> ErrorCodes { get; set; } = new();
    }
}
