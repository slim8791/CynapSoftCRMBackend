using System.ComponentModel.DataAnnotations;

namespace CynapCRM.Services.AuthAPI.Models.Dto
{
    public class ChangeRoleDto
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public UserRole NewRole { get; set; }
    }
}
