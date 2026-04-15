using System.ComponentModel.DataAnnotations;

namespace CynapCRM.Services.AuthAPI.Models.Dto
{
    public class ForgotPasswordDto
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        
    }
}
