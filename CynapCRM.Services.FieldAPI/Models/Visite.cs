using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CynapCRM.Services.FieldAPI.Models
{
    public class Visite
    {
        [Key]
        public int Id_Visite { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty; 


        [Required]
        public int Id_User_Delegue { get; set; } // 

        public int? Id_Medecin { get; set; }
        public int? Id_Pharmacien { get; set; }


        public int? Id_Tournee { get; set; } 

        [ForeignKey("Id_Tournee")]
        public virtual Tournee? Tournee { get; set; }

        // Une visite génère un seul rapport)
        public virtual Rapport_visite? Rapport { get; set; }
    }
}
