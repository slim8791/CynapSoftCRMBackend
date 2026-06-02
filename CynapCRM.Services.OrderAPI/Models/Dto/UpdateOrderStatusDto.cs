using System.ComponentModel.DataAnnotations;

namespace CynapCRM.Services.OrderAPI.Models.Dto
{
    public class UpdateOrderStatusDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Id_Commande invalide.")]
        public int Id_Commande { get; set; }

        [Required]
        public EtatCommande NouveauStatut { get; set; }
    }
}