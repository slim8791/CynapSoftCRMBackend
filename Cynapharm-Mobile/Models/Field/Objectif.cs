using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Field;

public class Objectif
{
    [JsonPropertyName("id_Objectif")]
    public int Id { get; set; }

    [JsonPropertyName("type")]
    public int TypeCode { get; set; }

    [JsonPropertyName("periode")]
    public int PeriodeCode { get; set; }

    [JsonPropertyName("valeurCible")]
    public int ValeurCible { get; set; }

    // Nullable so ViewModel's (o.ValeurActuelle ?? 0) keeps compiling unchanged
    [JsonPropertyName("valeurRealisee")]
    public int? ValeurActuelle { get; set; }

    [JsonPropertyName("id_User_Delegue")]
    public int IdDelegue { get; set; }

    [JsonPropertyName("dateDebut")]
    public DateTime DateDebut { get; set; }

    [JsonPropertyName("dateFin")]
    public DateTime DateFin { get; set; }

    [JsonIgnore]
    public string TypeObjectif => TypeCode switch
    {
        0 => "Visites",
        1 => "Chiffre d'affaires",
        2 => "Nouveaux clients",
        3 => "Fidélisation",
        _ => $"Type {TypeCode}"
    };

    [JsonIgnore]
    public string Periode => PeriodeCode switch
    {
        0 => "Mensuel",
        1 => "Trimestriel",
        2 => "Annuel",
        _ => $"Période {PeriodeCode}"
    };

    [JsonIgnore]
    public double ProgressValue =>
        ValeurCible > 0
            ? Math.Min(1.0, (ValeurActuelle ?? 0) / (double)ValeurCible)
            : 0;

    [JsonIgnore]
    public string ProgressLabel =>
        $"{ValeurActuelle ?? 0} / {ValeurCible}";
}
