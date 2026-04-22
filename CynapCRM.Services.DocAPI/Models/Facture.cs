using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CynapCRM.Services.DocAPI.Models
{
    public class Facture : Document
    {

        

        [Required]
        public decimal MontantHT { get; set; }

        [Required]
        public decimal MontantTTC { get; set; }

        [Required]
        public DateTime DateFacture { get; set; }
    }
}
