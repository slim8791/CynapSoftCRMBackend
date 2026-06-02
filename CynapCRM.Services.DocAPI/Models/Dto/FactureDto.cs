using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CynapCRM.Services.DocAPI.Models.Dto
{
    public class FactureDto : DocumentDto
    {
        [Required]
        [JsonPropertyName("montantHT")]
        public decimal MontantHT { get; set; }

        [Required]
        [JsonPropertyName("montantTTC")]
        public decimal MontantTTC { get; set; }

        [Required]
        [JsonPropertyName("dateFacture")]
        public DateTime DateFacture { get; set; }
    }
}
