using System.ComponentModel.DataAnnotations;

namespace CynapCRM.Services.SupportAPI.Models
{
    public enum TypeReclamation { Produit, Livraison, Facturation, Autre }
    public enum Gravite { Faible, Critique }
    public class Reclamation
    {
        [Key]
        public int Id_Reclamation { get; set; }

        [Required]
        public string Objet { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public DateTime DateDepot { get; set; } = DateTime.Now;

        // États de traitement
        public bool EstResolue { get; set; } = false;
        public string? SolutionApportee { get; set; }

        // Caractéristiques
        public TypeReclamation Type { get; set; } = TypeReclamation.Produit;
        public Gravite NiveauUrgence { get; set; } = Gravite.Faible;

        // IDs Externes (Pour savoir qui se plaint et de quoi)
        public int Id_User_Emetteur { get; set; } // Le client ou le délégué qui signale
        public int? Id_Commande { get; set; }     // Optionnel : lié à une commande précise
        public int? Id_Produit { get; set; }      // Optionnel : lié à un médicament précis
    }
}
