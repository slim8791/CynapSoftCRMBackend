using CynapCRM.Services.ProductAPI.Models.Dto;
using CynapCRM.Services.ProductAPI.Service.IService;

namespace CynapCRM.Services.ProductAPI.Service
{
    public class LotService : ILotService
    {
        public Task<bool> AdjustStockAsync(int productId, int quantityChange)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CanUpdateLotQuantityAsync(string numeroLot, int quantityChange)
        {
            throw new NotImplementedException();
        }

        public Task<LotDto> CreateUpdateLotAsync(LotDto lotDto)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteLotAsync(string numeroLot)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<LotDto>> GetLotsByProductIdAsync(int productId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<LotDto>> GetLotsNearExpirationAsync(int daysThreshold)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsLotExpiredAsync(string numeroLot)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateLotQuantityAsync(string numeroLot, int quantityChange)
        {
            throw new NotImplementedException();
        }
    }
}
