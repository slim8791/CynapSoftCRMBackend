using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Field;

public class Objectif
{
    [JsonPropertyName("id_Objectif")]
    public int Id { get; set; }

    [JsonPropertyName("id_User_Delegue")]
    public int DelegueId { get; set; }

    // Backend serializes TypeObjectif enum as int: 1=Visites, 2=ChiffreAffaires,
    // 3=NouveauxClients, 4=Fidelisation
    [JsonPropertyName("type")]
    public int TypeCode { get; set; }

    public string TypeObjectif => TypeCode switch
    {
        1 => "Visites",
        2 => "Chiffre d'affaires",
        3 => "Nouveaux clients",
        4 => "Fidélisation",
        _ => TypeCode > 0 ? $"Type {TypeCode}" : string.Empty
    };

    public decimal ValeurCible { get; set; }

    // Backend field is ValeurRealisee; mobile displays as ValeurActuelle
    [JsonPropertyName("valeurRealisee")]
    public decimal? ValeurActuelle { get; set; }

    // Backend serializes PeriodeObjectif enum as int: 1=Mensuel, 2=Trimestriel, 3=Annuel
    [JsonPropertyName("periode")]
    public int PeriodeCode { get; set; }

    public string Periode => PeriodeCode switch
    {
        1 => "Mensuel",
        2 => "Trimestriel",
        3 => "Annuel",
        _ => PeriodeCode > 0 ? $"Période {PeriodeCode}" : string.Empty
    };

    public double ProgressValue =>
        ValeurCible > 0
            ? Math.Min((double)(ValeurActuelle ?? 0) / (double)ValeurCible, 1.0)
            : 0;
}
