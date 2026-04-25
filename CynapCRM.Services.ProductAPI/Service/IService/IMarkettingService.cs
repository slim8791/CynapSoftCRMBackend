using CynapCRM.Services.ProductAPI.Models.Dto;

namespace CynapCRM.Services.ProductAPI.Service.IService
{

    public interface IMarkettingService
    {


        // Récupère tous les supports marketing d’un produit
        Task<IEnumerable<SupportMarketingDto>> GetSupportsByProductAsync(int productId);

        /// Récupère un support marketing par son identifiant
        Task<SupportMarketingDto?> GetSupportByIdAsync(int supportId);

        // Crée ou met à jour un support marketing
        Task<SupportMarketingDto> CreateOrUpdateSupportAsync(SupportMarketingDto supportDto);

        // Désactive un support marketing (suppression logique)
        Task<bool> DisableSupportAsync(int supportId);
        Task<bool> ActivateSupportAsync(int supportId);

        //  Fichiers marketing

        // Ajoute un fichier (PDF, image, vidéo…) à un support marketing
        Task<FichierDto> AddFileToSupportAsync(FichierDto fichierDto);

        // Supprime un fichier marketing
        Task<bool> DeleteFileAsync(int fichierId);

        // Récupère les fichiers associés à un support marketing
        Task<IEnumerable<FichierDto>> GetFilesBySupportAsync(int supportId);

        // 🔹 Visibilité / logique métier

        // Vérifie si un support marketing est actif et exploitable
        Task<bool> IsSupportActiveAsync(int supportId);

        // Récupère les supports visibles pour la vente
        // (délégués médicaux / clients)
        Task<IEnumerable<SupportMarketingDto>> GetVisibleSupportsByProductAsync(int productId);

        //  Campagnes marketing
        
        // Récupère les supports marketing associés à une campagne
        Task<IEnumerable<SupportMarketingDto>> GetSupportsByCampaignAsync(string campaignName);

        // Récupère toutes les campagnes marketing existantes
        Task<IEnumerable<string>> GetCampaignsAsync();
    }
}