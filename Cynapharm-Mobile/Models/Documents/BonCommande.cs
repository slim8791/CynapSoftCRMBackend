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
    public decimal? MontantTotal { get; internal set; }
    public string Statut { get; internal set; }
}
