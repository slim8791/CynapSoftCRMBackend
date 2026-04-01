using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CynapCRM.Services.ProductAPI.Models
{
    public class Fichier
    {
        [Key]
        public int Id_Fichier { get; set; }

        [Required]
        public string NomFichier { get; set; } = string.Empty; 

        [Required]
        public string Url { get; set; } = string.Empty; 

        public string Extension { get; set; } = string.Empty; 

        public long Taille { get; set; } 

        // Relation avec le Support Marketing 
        [Required]
        public int Id_Support { get; set; }

        [ForeignKey("Id_Support")]
        public virtual Support_Marketting? Support { get; set; }
    }
}