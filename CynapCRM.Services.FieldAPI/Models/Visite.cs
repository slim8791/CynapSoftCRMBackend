using CynapCRM.Services.FieldAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CynapCRM.Services.FieldAPI.Models
{
    public class Visite
    {
        [Key]
        public int Id_Visite { get; set; }

        [Required]
        public DateTime DateVisite { get; set; }

        [Required]
        public VisiteType Type { get; set; }

        // Délégué
        [Required]
        public int Id_User_Delegue { get; set; }

        // Cible (selon le type)
        public int? Id_Medecin { get; set; }
        public int? Id_Pharmacien { get; set; }

        // ✅ Lien vers PlanningVisite
        public int? Id_Planning { get; set; }

        [ForeignKey(nameof(Id_Planning))]
        public virtual Planning_Visite? Planning { get; set; }

        // Région optionnelle
        public int? Id_Region { get; set; }

        // ✅ Une visite → un rapport
        public virtual Rapport_Visite? Rapport { get; set; }

        // Statut simple
        public bool IsCompleted { get; set; } = false;
    }
}
