using CynapCRM.Services.ProductAPI.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.ProductAPI.Service.IService
{
    public interface IProductService
    {
        // Gestion des Produits
        Task<IEnumerable<ProduitDto>> GetProductsAsync();
        Task<ProduitDto> GetProductByIdAsync(int produitId);
        Task<ProduitDto> CreateUpdateProductAsync(ProduitDto produitDto);
        Task<bool> DeleteProductAsync(int produitId);

        // Gestion des lots

        // Récupérer tous les lots d'un médicament (pour voir les dates d'expiration)
        Task<IEnumerable<LotDto>> GetLotsByProductIdAsync(int productId);

        // Ajouter un nouveau lot à l'inventaire
        Task<LotDto> CreateUpdateLotAsync(LotDto lotDto);

        // Supprimer un lot (en cas d'erreur de saisie)
        Task<bool> DeleteLotAsync(string numeroLot);


        // Gestion des promotions
        // Récupérer toutes les promos actives
        Task<IEnumerable<PromotionDto>> GetPromotionsAsync();

        // Créer ou Modifier une promo (ex: -20% sur le Lot X)
        Task<PromotionDto> CreateUpdatePromotionAsync(PromotionDto promotionDto);

        // Supprimer une promotion
        Task<bool> DeletePromotionAsync(int promotionId);


        // Gestion marketing et supports

        // Récupérer les supports (vidéos, PDF) d'un produit
        Task<IEnumerable<SupportMarketingDto>> GetSupportsByProductIdAsync(int productId);

        // Créer un nouveau support (ex: "Campagne Printemps 2026")
        Task<SupportMarketingDto> CreateUpdateSupportAsync(SupportMarketingDto supportDto);

        // Ajouter un fichier physique (Url, Nom) à un support
        Task<FichierDto> AddFichierToSupportAsync(FichierDto fichierDto);

        // Supprimer un fichier technique
        Task<bool> DeleteFichierAsync(int fichierId);
    }
}
