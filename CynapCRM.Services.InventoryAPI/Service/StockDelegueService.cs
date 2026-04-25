using AutoMapper;
using CynapCRM.Services.InventoryAPI.Data;
using CynapCRM.Services.InventoryAPI.Models;
using CynapCRM.Services.InventoryAPI.Models.Dto;
using CynapCRM.Services.InventoryAPI.Service.IService;
using Microsoft.EntityFrameworkCore;

namespace CynapCRM.Services.InventoryAPI.Service
{
    public class StockDelegueService : IStockDelegueService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        public StockDelegueService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;

        }
        public async Task<IEnumerable<StockDelegueDto>> GetAllStocksAsync(int pageNumber, int pageSize)
        {

            var stocks = await _db.StocksDelegues
                            .AsNoTracking()
                            .Where(s => !s.IsDeleted)
                            .OrderByDescending(s => s.DateCreation)
                            .Skip((pageNumber - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();
            return _mapper.Map<IEnumerable<StockDelegueDto>>(stocks);
        }

        public async Task<StockDelegueDto?> CreateUpdateStockAsync(StockDelegueDto dto)
        {

            var stock = new Stock_Delegue
            {
                Id_User_Delegue = dto.Id_User_Delegue,
                Id_Produit = dto.Id_Produit,
                NumeroLot = dto.NumeroLot,
                QteDisponible = dto.QteDisponible,
                QteReservee = 0,
                DateCreation = DateTime.UtcNow,
                IsDeleted = false
            };

            _db.StocksDelegues.Add(stock);
            await _db.SaveChangesAsync();

            return _mapper.Map<StockDelegueDto>(stock);

        }
        public async Task<StockDelegueDto?> GetStockByIdAsync(int idStock)
        {

            var stock = await _db.StocksDelegues
                            .AsNoTracking()
                            .FirstOrDefaultAsync(s => s.Id_stock == idStock && !s.IsDeleted);
            if (stock == null)
            {
                return null;
            }
            return _mapper.Map<StockDelegueDto>(stock);
        }
        public async Task<IEnumerable<StockDelegueDto>> GetStocksByDelegueAsync(int idDelegue)
        {

            var stocks = await _db.StocksDelegues
                            .AsNoTracking()
                            .Where(s => s.Id_User_Delegue == idDelegue && !s.IsDeleted)
                            .OrderByDescending(s => s.DateCreation)
                            .ToListAsync();

            return _mapper.Map<IEnumerable<StockDelegueDto>>(stocks);
        }
        public async Task<IEnumerable<StockDelegueDto>> GetStockByProduitAsync(int idProduit)
        {

            var stocks = await _db.StocksDelegues
                            .AsNoTracking()
                            .Where(s => s.Id_Produit == idProduit && !s.IsDeleted)
                            .ToListAsync();

            return _mapper.Map<IEnumerable<StockDelegueDto>>(stocks);
        }
        public async Task<StockDelegueDto?> GetStockByLotAsync(string numeroLot)
        {

            var stock = await _db.StocksDelegues
                            .AsNoTracking()
                            .FirstOrDefaultAsync(s =>s.NumeroLot == numeroLot && !s.IsDeleted);

            if (stock == null)
            {
                return null;
            }
            return _mapper.Map<StockDelegueDto>(stock);
        }
        public async Task<bool> DeleteStockAsync(int idStock, StockType type)
        {

            if (type != StockType.Delegue)
                return false; 

            var stock = await _db.StocksDelegues
                .FirstOrDefaultAsync(s => s.Id_stock == idStock);

            if (stock == null)
                return false;

            stock.IsDeleted = true;
            await _db.SaveChangesAsync();
            return true;

        }    
    }
}
