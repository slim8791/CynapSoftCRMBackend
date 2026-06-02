using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Documents;

public class Facture
{
    [JsonPropertyName("numero_Doc")]
    public int Id { get; set; }

    [JsonPropertyName("nom_Doc")]
    public string NumeroFacture { get; set; } = string.Empty;

    [JsonPropertyName("dateFacture")]
    public DateTime DateFacture { get; set; }

    [JsonPropertyName("id_Commande")]
    public int CommandeId { get; set; }

    [JsonPropertyName("montantHT")]
    public decimal MontantHT  { get; set; }

    [JsonPropertyName("montantTTC")]
    public decimal MontantTTC { get; set; }

    [JsonPropertyName("cloudinaryUrl")]
    public string? CloudinaryUrl { get; set; }

    [JsonIgnore]
    public decimal TVA => MontantTTC - MontantHT;

    public string Statut { get; internal set; }
}
