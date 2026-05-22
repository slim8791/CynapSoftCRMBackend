using System.Globalization;
using System.Text.Json.Serialization;

namespace Cynapharm_Mobile.Models.Products;

public class Product
{
    // Fixed format: period as decimal separator, space as thousands separator, 3 decimal places.
    private static readonly NumberFormatInfo _tndFormat = new()
    {
        NumberDecimalSeparator = ".",
        NumberGroupSeparator   = " ",
        NumberDecimalDigits    = 3
    };

    [JsonPropertyName("id_Produit")]
    public int Id { get; set; }

    public string Reference { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Categorie { get; set; } = string.Empty;

    [JsonPropertyName("prixVente")]
    public decimal PrixUnitaire { get; set; }

    public bool   IsPriceDefined => PrixUnitaire > 0;
    public string PrixDisplay    => PrixUnitaire > 0
        ? PrixUnitaire.ToString("N", _tndFormat) + " TND"
        : "Prix non défini";

    public string? ImageUrl { get; set; }

    [JsonPropertyName("isActive")]
    public bool Actif { get; set; }

    public bool IsArchived { get; set; }

    public List<SupportMarketing>? Supports { get; set; }
}

public class SupportMarketing
{
    public string Type { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? CampaignName { get; set; }
    public List<Fichier>? Fichiers { get; set; }
}

public class Fichier
{
    public string NomFichier { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
}
