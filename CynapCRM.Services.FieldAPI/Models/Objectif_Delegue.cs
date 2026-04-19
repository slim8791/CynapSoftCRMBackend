using System.ComponentModel.DataAnnotations;

namespace CynapCRM.Services.FieldAPI.Models
{
    public class Objectif_Delegue
    {
        [Key]
        public int Id_Objectif { get; set; }
        [Required]
        public string Type { get; set; } = string.Empty;
        [Required]
        public int ValeurCible { get; set; }
        [Required]
        public string Periode { get; set; } = string.Empty;
        [Required]
        public int Id_User_Delegue { get; set; }
    }
}
