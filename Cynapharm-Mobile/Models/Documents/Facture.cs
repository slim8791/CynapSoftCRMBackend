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

    public decimal MontantHT { get; set; }

    public decimal MontantTTC { get; set; }

    [JsonIgnore]
    public decimal TVA => MontantTTC - MontantHT;

    public string Statut { get; internal set; }
}
