using CynapCRM.Services.ProductAPI.Models.Dto;

namespace CynapCRM.Services.ProductAPI.Service.IService
{
    public interface ILotService
    {

        Task<LotDto?> GetLotByNumeroAsync(string numeroLot);

        Task<IEnumerable<LotDto>> GetLotsByProductIdAsync(int productId);
        // Récupère les lots actifs
        Task<IEnumerable<LotDto>> GetAvailableLotsAsync(int productId);

        Task<LotDto> CreateOrUpdateLotAsync(LotDto lotDto);
        Task<bool> DeleteLotAsync(string numeroLot);


        // Logique métier sur lots

        /// <summary>
        /// Ajuste le stock global d’un produit en répartissant la quantité
        /// sur ses lots disponibles (FIFO / FEFO)
        /// </summary>

        Task<bool> AdjustStockAsync(int productId, int quantityChange);
        
        // Modifie la quantité d’un lot spécifique

        Task<bool> UpdateLotQuantityAsync(string numeroLot, int quantityChange);

        // Vérifie si un lot est en rupture de stock

        Task<bool> IsLotOutOfStockAsync(string numeroLot);
        // Vérifier si un lot est expiré

        Task<bool> IsLotExpiredAsync(string numeroLot);
        // recupérer les lots proches de l'expiration
        Task<IEnumerable<LotDto>> GetLotsNearExpirationAsync(int daysThreshold);
        // recupérer les lots expirés 
        Task<IEnumerable<LotDto>> GetExpiredLotsAsync();
        // recupérer tous les lots
        Task<IEnumerable<LotDto>> GetAllLotsAsync();

    }
}
