using AutoMapper;
using CynapCRM.Services.ProductAPI.Data;
using CynapCRM.Services.ProductAPI.Models;
using CynapCRM.Services.ProductAPI.Models.Dto;
using CynapCRM.Services.ProductAPI.Service.IService;
using Microsoft.EntityFrameworkCore;

namespace CynapCRM.Services.ProductAPI.Service
{
    public class LotService : ILotService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        public LotService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<bool> AdjustStockAsync(int productId, int quantityChange)
        {

            if (quantityChange == 0) return true;

            var lots = await _db.Lots
                .Where(l =>
                    l.Id_Produit == productId &&
                    l.Quantite > 0 &&
                    l.DateExpiration > DateTime.UtcNow)
                .OrderBy(l => l.DateExpiration) // FEFO
                .ToListAsync();

            if (!lots.Any()) return false;

            var remaining = Math.Abs(quantityChange);

            foreach (var lot in lots)
            {
                if (remaining <= 0) break;

                var deduction = Math.Min(lot.Quantite, remaining);
                lot.Quantite -= deduction;
                remaining -= deduction;
            }

            await _db.SaveChangesAsync();
            return remaining == 0;
        }

        

        public async Task<bool> DeleteLotAsync(string numeroLot)
        {

            var lot = await _db.Lots.FindAsync(numeroLot);
            if (lot == null) return false;

            _db.Lots.Remove(lot);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<LotDto>> GetAvailableLotsAsync(int productId)
        {

            var today = DateTime.UtcNow;

            var lots = await _db.Lots
                .Where(l =>
                    l.Id_Produit == productId &&
                    l.Quantite > 0 &&
                    l.DateExpiration > today)
                .OrderBy(l => l.DateExpiration) 
                .ToListAsync();

            return _mapper.Map<IEnumerable<LotDto>>(lots);
        }

        public async Task<IEnumerable<LotDto>> GetExpiredLotsAsync()
        {

            var lots = await _db.Lots
                            .Where(l => l.DateExpiration <= DateTime.UtcNow)
                            .ToListAsync();

            return _mapper.Map<IEnumerable<LotDto>>(lots);
        }

        public async Task<LotDto?> GetLotByNumeroAsync(string numeroLot)
        {

            var lot = await _db.Lots
                            .Include(l => l.Promotions)
                            .FirstOrDefaultAsync(l => l.Numero == numeroLot);

            return lot == null ? null : _mapper.Map<LotDto>(lot);
        }
        
        public async Task<IEnumerable<LotDto>> GetLotsByProductIdAsync(int productId)
        {

            var lots = await _db.Lots
                            .Where(l => l.Id_Produit == productId)
                            .Include(l => l.Promotions)
                            .ToListAsync();

            return _mapper.Map<IEnumerable<LotDto>>(lots);
        }

        public async Task<IEnumerable<LotDto>> GetLotsNearExpirationAsync(int daysThreshold)
        {

            var limitDate = DateTime.UtcNow.AddDays(daysThreshold);

            var lots = await _db.Lots
                .Where(l =>
                    l.DateExpiration > DateTime.UtcNow &&
                    l.DateExpiration <= limitDate)
                .OrderBy(l => l.DateExpiration)
                .ToListAsync();

            return _mapper.Map<IEnumerable<LotDto>>(lots);
        }

        public async Task<bool> IsLotExpiredAsync(string numeroLot)
        {

            var lot = await _db.Lots.FindAsync(numeroLot);
            return lot == null || lot.DateExpiration <= DateTime.UtcNow;
        }

        public async Task<bool> IsLotOutOfStockAsync(string numeroLot)
        {

            var lot = await _db.Lots.FindAsync(numeroLot);
            return lot == null || lot.Quantite <= 0;
        }

        public async Task<bool> UpdateLotQuantityAsync(string numeroLot, int quantityChange)
        {

            var lot = await _db.Lots.FindAsync(numeroLot);
            if (lot == null) return false;

            if (lot.Quantite + quantityChange < 0)
                return false;

            lot.Quantite += quantityChange;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<LotDto> CreateOrUpdateLotAsync(LotDto lotDto)
        {

            var lot = await _db.Lots
                            .Include(l => l.Promotions)
                            .FirstOrDefaultAsync(l => l.Numero == lotDto.Numero);
            if (lot == null)
            {
                lot = _mapper.Map<Lot>(lotDto);
                _db.Lots.Add(lot);
            }
            else
            {
                _mapper.Map(lotDto, lot);
            }

            await _db.SaveChangesAsync();
            return _mapper.Map<LotDto>(lot);
        }
        
        
    }
}
