namespace Cynapharm_Mobile.Models.Field;

public class Rapport
{
    public int Id { get; set; }
    public int VisiteId { get; set; }
    public string Contenu { get; set; } = string.Empty;
    public string? ProduitsDiscutes { get; set; }
    public string Resultat { get; set; } = string.Empty;
    public DateTime DateSoumission { get; set; }

    // Geolocation proof-of-presence captured at submit time
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
