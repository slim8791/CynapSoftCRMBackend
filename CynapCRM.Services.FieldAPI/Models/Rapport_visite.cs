using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CynapCRM.Services.FieldAPI.Models
{
    public class Rapport_visite
    {
        [Key]
        public int Id_Rapport { get; set; }
        public string Commentaire { get; set; } = string.Empty;
        public string Resultat { get; set; } = string.Empty;
        public DateTime Date { get; set; }

        // --- RELATION ESSENTIELLE : LA VISITE ---

        [Required]
        public int Id_Visite { get; set; }

        [ForeignKey("Id_Visite")]
        public virtual Visite? Visite { get; set; }

        // --- RELATION LOGIQUE : L'AUTEUR (Délégué) ---

        [Required]
        public int Id_User_Delegue { get; set; }
    }
}
