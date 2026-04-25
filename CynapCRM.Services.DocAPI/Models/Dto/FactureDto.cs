using System.ComponentModel.DataAnnotations;

namespace CynapCRM.Services.DocAPI.Models.Dto
{
    public class FactureDto : DocumentDto
    {
        public int Numero_Doc { get; set; }

        [Required]
        public decimal MontantHT { get; set; }

        [Required]
        public decimal MontantTTC { get; set; }

        [Required]
        public DateTime DateFacture { get; set; }
    }
}
