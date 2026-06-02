using System.Text.Json.Serialization;

namespace CynapCRM.Services.DocAPI.Models.Dto
{
    public class DocumentDto
    {
        [JsonPropertyName("numero_Doc")]
        public int Numero_Doc { get; set; }

        [JsonPropertyName("nom_Doc")]
        public string Nom_Doc { get; set; } = string.Empty;

        [JsonPropertyName("dateCreation")]
        public DateTime DateCreation { get; set; }

        [JsonPropertyName("id_Commande")]
        public int Id_Commande { get; set; }

        [JsonPropertyName("id_Client")]
        public int? Id_Client { get; set; }

        [JsonPropertyName("typeDocument")]
        public string TypeDocument { get; set; } = string.Empty;

        [JsonPropertyName("cloudinaryUrl")]
        public string? CloudinaryUrl { get; set; }
    }
}
