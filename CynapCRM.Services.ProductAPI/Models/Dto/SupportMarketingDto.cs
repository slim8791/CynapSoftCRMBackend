namespace CynapCRM.Services.ProductAPI.Models.Dto
{
    public class SupportMarketingDto
    {

        public int Id_SupportMarketting { get; set; }
        public string Type { get; set; } = string.Empty;

        public int Id_Produit { get; set; }

        // Logique métier
        public bool IsActive { get; set; } = true;
        public string? CampaignName { get; set; }

        public IEnumerable<FichierDto>? Fichiers { get; set; }

    }
}
