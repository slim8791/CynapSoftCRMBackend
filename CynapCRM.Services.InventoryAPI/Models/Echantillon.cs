using System.ComponentModel.DataAnnotations;

namespace CynapCRM.Services.InventoryAPI.Models
{
    public class Echantillon
    {
        [Key]
        public int Id_Distribution { get; set; }

        [Required]
        public int Qte { get; set; }

        [Required]
        public DateTime DateDistribution { get; set; }

        // IDs externes pour la traçabilité
        public int Id_Medecin { get; set; }
        public string NumeroLot { get; set; } = string.Empty;
        public int Id_User_Delegue { get; set; }
    }
}
