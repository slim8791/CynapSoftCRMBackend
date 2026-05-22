namespace CynapCRM.Services.AuthAPI.Models.Dto
{
    public class LoginRequestDto
    {
        public string UserName { get; set; } = string.Empty; 
        public string Password { get; set; } = string.Empty;
        public string? TurnstileToken { get; set; }
    }
}
