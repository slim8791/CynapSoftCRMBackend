using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Field;

public class PerformanceDto
{
    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("valeurCible")]
    public int ValeurCible { get; set; }

    [JsonPropertyName("valeurRealisee")]
    public int ValeurRealisee { get; set; }

    [JsonPropertyName("pourcentage")]
    public double Pourcentage { get; set; }

    [JsonIgnore]
    public string TypeLabel => Type switch
    {
        0 => "Visites",
        1 => "Chiffre d'affaires",
        2 => "Nouveaux clients",
        3 => "Fidélisation",
        _ => $"Type {Type}"
    };
}
