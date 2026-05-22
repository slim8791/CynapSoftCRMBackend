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

    // TVA not in backend DTO — kept for XAML compiled-binding compatibility
    public decimal TVA { get; set; }

    public decimal MontantTTC { get; set; }

    // Statut not in backend DTO — kept for XAML compiled-binding compatibility
    public string Statut { get; set; } = string.Empty;
}
