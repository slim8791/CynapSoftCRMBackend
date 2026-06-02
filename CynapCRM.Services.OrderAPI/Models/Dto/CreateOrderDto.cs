using System.ComponentModel.DataAnnotations;

namespace CynapCRM.Services.OrderAPI.Models.Dto
{
    public class CreateOrderDto
    {
        public int Id_Client { get; set; }

        // FIX: validation — au moins une ligne requise
        [Required]
        [MinLength(1, ErrorMessage = "La commande doit contenir au moins une ligne.")]
        public List<CreateOrUpdateLigneCommandeDto> Lignes { get; set; } = new();

        public bool IsFinalValidation { get; set; }

        public const decimal TauxTVA = 0.19m;
    }
}