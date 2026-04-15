using System.ComponentModel.DataAnnotations;

namespace CynapCRM.Services.AuthAPI.Models.Dto
{
    public class ChangePasswordDto
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, MinLength(6)]
        public string CurrentPassword { get; set; }

        [Required, MinLength(6)]
        public string NewPassword { get; set; }
    }
}
