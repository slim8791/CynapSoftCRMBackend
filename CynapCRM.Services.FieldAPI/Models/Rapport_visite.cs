
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CynapCRM.Services.FieldAPI.Models
{
    public class Rapport_Visite
    {
        [Key]
        public int Id_Rapport { get; set; }

        [Required]
        public string Commentaire { get; set; } = string.Empty;

        [Required]
        public string Resultat { get; set; } = string.Empty;

        [Required]
        public DateTime DateRapport { get; set; }

        // Relation obligatoire avec Visite
        [Required]
        public int Id_Visite { get; set; }

        [ForeignKey(nameof(Id_Visite))]
        public virtual Visite Visite { get; set; } = null!;

        [Required]
        public int Id_User_Delegue { get; set; }
    }
}