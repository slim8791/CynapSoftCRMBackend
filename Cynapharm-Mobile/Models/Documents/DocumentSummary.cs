using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Documents;

public class DocumentSummary
{
    [JsonPropertyName("numero_Doc")]
    public int Id { get; set; }

    [JsonPropertyName("nom_Doc")]
    public string Numero { get; set; } = string.Empty;

    [JsonPropertyName("dateCreation")]
    public DateTime Date { get; set; }

    [JsonPropertyName("typeDocument")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("id_Commande")]
    public int CommandeId { get; set; }

    // url_Document — present when backend includes it (Cloudinary or storage link)
    [JsonPropertyName("cloudinaryUrl")]
    public string? Url { get; set; }

    // Statut not in backend DocumentDto — kept for XAML compiled-binding compatibility
    public string Statut { get; set; } = string.Empty;

    // Montant not in backend DocumentDto — kept for XAML compiled-binding compatibility
    public decimal? Montant { get; set; }
}
