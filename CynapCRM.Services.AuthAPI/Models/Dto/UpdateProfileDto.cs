using System.ComponentModel.DataAnnotations;

namespace CynapCRM.Services.AuthAPI.Models.Dto
{
    public class UpdateProfileDto
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Adresse { get; set; }
        public int? IdRegion { get; set; }
    }
}
