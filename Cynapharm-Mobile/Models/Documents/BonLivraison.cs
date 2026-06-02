using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Documents;

public class BonLivraison
{
    [JsonPropertyName("numero_Doc")]
    public int Id { get; set; }

    [JsonPropertyName("nom_Doc")]
    public string NumeroBon { get; set; } = string.Empty;

    [JsonPropertyName("dateCreation")]
    public DateTime DateLivraison { get; set; }

    [JsonPropertyName("id_Commande")]
    public int CommandeId { get; set; }

    [JsonPropertyName("cloudinaryUrl")]
    public string? CloudinaryUrl { get; set; }

    public string Statut { get; internal set; }
}
