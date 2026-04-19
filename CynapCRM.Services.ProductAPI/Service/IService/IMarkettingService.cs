using CynapCRM.Services.ProductAPI.Models.Dto;

namespace CynapCRM.Services.ProductAPI.Service.IService
{
    /// <summary>
    /// Service métier responsable de la gestion des supports marketing
    /// (documents, visuels, campagnes commerciales)
    /// </summary>
    public interface IMarkettingService
    {
        // ==================================================
        // 🔹 Supports marketing
        // ==================================================

        /// <summary>
        /// Récupère tous les supports marketing d’un produit
        /// </summary>
        Task<IEnumerable<SupportMarketingDto>> GetSupportsByProductAsync(int productId);

        /// <summary>
        /// Récupère un support marketing par son identifiant
        /// </summary>
        Task<SupportMarketingDto?> GetSupportByIdAsync(int supportId);

        /// <summary>
        /// Crée ou met à jour un support marketing
        /// </summary>
        Task<SupportMarketingDto> CreateOrUpdateSupportAsync(SupportMarketingDto supportDto);

        /// <summary>
        /// Désactive un support marketing (suppression logique)
        /// </summary>
        Task<bool> DisableSupportAsync(int supportId);

        // ==================================================
        // 🔹 Fichiers marketing
        // ==================================================

        /// <summary>
        /// Ajoute un fichier (PDF, image, vidéo…) à un support marketing
        /// </summary>
        Task<FichierDto> AddFileToSupportAsync(FichierDto fichierDto);

        /// <summary>
        /// Supprime un fichier marketing
        /// </summary>
        Task<bool> DeleteFileAsync(int fichierId);

        /// <summary>
        /// Récupère les fichiers associés à un support marketing
        /// </summary>
        Task<IEnumerable<FichierDto>> GetFilesBySupportAsync(int supportId);

        // ==================================================
        // 🔹 Visibilité & logique métier
        // ==================================================

        /// <summary>
        /// Vérifie si un support marketing est actif et exploitable
        /// </summary>
        Task<bool> IsSupportActiveAsync(int supportId);

        /// <summary>
        /// Récupère les supports visibles pour la vente
        /// (délégués médicaux / clients)
        /// </summary>
        Task<IEnumerable<SupportMarketingDto>> GetVisibleSupportsByProductAsync(int productId);

        // ==================================================
        // 🔹 Campagnes marketing
        // ==================================================

        /// <summary>
        /// Récupère les supports marketing associés à une campagne
        /// </summary>
        Task<IEnumerable<SupportMarketingDto>> GetSupportsByCampaignAsync(string campaignName);

        /// <summary>
        /// Récupère toutes les campagnes marketing existantes
        /// </summary>
        Task<IEnumerable<string>> GetCampaignsAsync();
    }
}