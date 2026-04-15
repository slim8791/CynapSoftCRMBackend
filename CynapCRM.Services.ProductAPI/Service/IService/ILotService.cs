using CynapCRM.Services.ProductAPI.Models.Dto;

namespace CynapCRM.Services.ProductAPI.Service.IService
{
    public interface ILotService
    {
        // ================================
        // 🔹 Gestion des Lots
        // ================================
        Task<IEnumerable<LotDto>> GetLotsByProductIdAsync(int productId);
        Task<LotDto> CreateUpdateLotAsync(LotDto lotDto);
        Task<bool> DeleteLotAsync(string numeroLot);

        // 🔥 Logique métier sur lots
        Task<bool> AdjustStockAsync(int productId, int quantityChange);
        Task<bool> CanUpdateLotQuantityAsync(string numeroLot, int quantityChange);
        Task<bool> UpdateLotQuantityAsync(string numeroLot, int quantityChange);
        Task<bool> IsLotExpiredAsync(string numeroLot);
        Task<IEnumerable<LotDto>> GetLotsNearExpirationAsync(int daysThreshold);
    }
}
