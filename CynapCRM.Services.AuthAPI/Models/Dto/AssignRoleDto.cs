using System.ComponentModel.DataAnnotations;

namespace CynapCRM.Services.AuthAPI.Models.Dto
{
    public class AssignRoleDto
    {
        [Required(ErrorMessage = "L'ID utilisateur est obligatoire.")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Le rôle est obligatoire.")]
        public string Role { get; set; }
    }
}
