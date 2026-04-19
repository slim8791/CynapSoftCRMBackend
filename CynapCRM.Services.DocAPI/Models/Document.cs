using System.ComponentModel.DataAnnotations;

namespace CynapCRM.Services.DocAPI.Models
{
    public class Document
    {
        [Key]
        public int Numero_Doc { get; set; }

        [Required]
        public string Nom_Doc { get; set; } = string.Empty;


        public string? ContentType { get; set; }
        public DateTime DateCreation { get; set; } = DateTime.Now;

        // ID de la commande associée (vient de OrderAPI)
        public int Id_Commande { get; set; }
        // optionnel : référence à l'utilisateur qui a créé le document MMMM
        public int? Id_Client { get; set; }
        public bool IsDeleted { get; internal set; } = false;
    }
}
