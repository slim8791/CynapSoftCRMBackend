using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Documents;

public class BonCommande
{
    [JsonPropertyName("numero_Doc")]
    public int Id { get; set; }

    [JsonPropertyName("nom_Doc")]
    public string NumeroBon { get; set; } = string.Empty;

    [JsonPropertyName("dateCreation")]
    public DateTime DateEmission { get; set; }

    [JsonPropertyName("id_Commande")]
    public int CommandeId { get; set; }

    // MontantTotal not in backend DTO — kept for XAML compiled-binding compatibility
    public decimal MontantTotal { get; set; }

    // Statut not in backend DTO — kept for XAML compiled-binding compatibility
    public string Statut { get; set; } = string.Empty;
}
