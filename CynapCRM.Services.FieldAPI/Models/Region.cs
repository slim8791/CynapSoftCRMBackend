using System.ComponentModel.DataAnnotations;

namespace CynapCRM.Services.FieldAPI.Models
{
    public class Region
    {
        [Key]
        public int Id_Region { get; set; }
        public string NomRegion { get; set; } = string.Empty;
        public string CodePostal { get; set; } = string.Empty;

        [Required]
        public int Id_User_Delegue { get; set; }
    }
}
