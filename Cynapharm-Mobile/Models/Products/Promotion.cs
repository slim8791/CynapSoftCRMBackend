using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Products;
public class Promotion
{
    [JsonPropertyName("id_Promo")]
    public int IdPromo { get; set; }

    [JsonPropertyName("numeroLot")]
    public string? NumeroLot { get; set; }

    [JsonPropertyName("codePromo")]
    public string CodePromo { get; set; } = string.Empty;

    [JsonPropertyName("typePromotion")]
    public string TypePromotion { get; set; } = string.Empty;

    [JsonPropertyName("pourcentage")]
    public float? Pourcentage { get; set; }

    [JsonPropertyName("quantiteGratuite")]
    public int? QuantiteGratuite { get; set; }

    [JsonPropertyName("seuilAchat")]
    public decimal? SeuilAchat { get; set; }

    [JsonPropertyName("dateDebut")]
    public DateTime DateDebut { get; set; }

    [JsonPropertyName("dateExpiration")]
    public DateTime DateExpiration { get; set; }

    [JsonPropertyName("estActive")]
    public bool EstActive { get; set; }

    [JsonPropertyName("porteeSurTousLesLots")]
    public bool PorteeSurTousLesLots { get; set; }

    [JsonPropertyName("isExpired")]
    public bool IsExpired { get; set; }
    public object DateFin { get; internal set; }
    public int? RemisePourcentage { get; internal set; }
    public string Titre { get; internal set; }
    public int? ProductId { get; internal set; }
    public int Id { get; internal set; }
}
