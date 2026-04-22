using CynapCRM.Services.ProductAPI.Models.Dto;

namespace CynapCRM.Services.ProductAPI.Service.IService
{
    /// <summary>
    /// Service métier responsable de la gestion et de l’application
    /// des promotions commerciales
    /// </summary>
    public interface IPromoService
    {
        //  Gestion des promotions

        /// <summary>
        /// Récupère toutes les promotions (actives et inactives)
        /// </summary>
        Task<IEnumerable<PromotionDto>> GetAllPromotionsAsync();

        /// <summary>
        /// Récupère une promotion par son identifiant
        /// </summary>
        Task<PromotionDto?> GetPromotionByIdAsync(int promotionId);

        /// <summary>
        /// Crée ou met à jour une promotion
        /// </summary>
        Task<PromotionDto> CreateOrUpdatePromotionAsync(PromotionDto promotionDto);

        /// <summary>
        /// Supprime une promotion (logique ou physique selon la règle métier)
        /// </summary>
        Task<bool> DeletePromotionAsync(int promotionId);

        // Application de la promotion

        /// <summary>
        /// Applique la meilleure promotion valide sur un prix donné
        /// </summary>
        Task<decimal> ApplyBestPromotionAsync(int productId, decimal initialPrice);

        /// <summary>
        /// Indique si un produit possède au moins une promotion active
        /// </summary>
        Task<bool> IsProductInPromotionAsync(int productId);

        //  Consultation par contexte

        /// <summary>
        /// Récupère les promotions liées à un produit
        /// </summary>
        Task<IEnumerable<PromotionDto>> GetPromotionsByProductAsync(int productId);

        /// <summary>
        /// Récupère les promotions liées à un lot
        /// </summary>
        Task<IEnumerable<PromotionDto>> GetPromotionsByLotAsync(string numeroLot);

        //  Validation métier

        /// <summary>
        /// Vérifie si une promotion est valide
        /// (date, activation, cohérence métier)
        /// </summary>
        Task<bool> IsPromotionValidAsync(int promotionId);

        /// <summary>
        /// Vérifie si une promotion est applicable à une date donnée
        /// </summary>
        Task<bool> IsPromotionApplicableAsync(int promotionId, DateTime referenceDate);

        //  Indicateurs // pilotage

        /// <summary>
        /// Taux de couverture promotionnelle
        /// (% de produits ayant au moins une promotion active)
        /// </summary>
        Task<double> GetPromotionCoverageRateAsync();

        /// <summary>
        /// Nombre de promotions actives
        /// </summary>
        Task<int> GetActivePromotionsCountAsync();
    }
}