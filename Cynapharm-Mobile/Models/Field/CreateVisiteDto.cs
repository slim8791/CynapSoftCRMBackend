using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Field;

public class CreateVisiteDto
{
    [JsonPropertyName("idVisite")]     public int      IdVisite     { get; set; }
    [JsonPropertyName("dateVisite")]   public DateTime DateVisite   { get; set; }
    [JsonPropertyName("type")]         public int      Type         { get; set; }  // Medecin=1, Pharmacien=2, Autre=3
    [JsonPropertyName("idMedecin")]    public int?     IdMedecin    { get; set; }
    [JsonPropertyName("idPharmacien")] public int?     IdPharmacien { get; set; }
    [JsonPropertyName("idPlanning")]   public int?     IdPlanning   { get; set; }
    [JsonPropertyName("id_User_Delegue")] public int   IdDelegue    { get; set; }
}
