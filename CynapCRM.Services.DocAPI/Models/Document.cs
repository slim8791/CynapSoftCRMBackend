using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CynapCRM.Services.DocAPI.Models
{
    public class Document
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int Numero_Doc { get; set; }

        [Required]
        public string Nom_Doc { get; set; } = string.Empty;

        public DateTime DateCreation { get; set; } = DateTime.UtcNow;

        // Refers to OrderAPI
        [Required]
        public int Id_Commande { get; set; }

        // Client 
        public int? Id_Client { get; set; }

        [Required]
        public string TypeDocument { get; set; } = string.Empty;

        public bool IsDeleted { get; set; } = false;

    }
}
