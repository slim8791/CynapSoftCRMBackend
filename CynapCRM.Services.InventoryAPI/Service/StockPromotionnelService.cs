using CynapCRM.Services.InventoryAPI.Models;
using CynapCRM.Services.InventoryAPI.Models.Dto;
using CynapCRM.Services.InventoryAPI.Service.IService;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CynapCRM.Services.InventoryAPI.Data;

namespace CynapCRM.Services.InventoryAPI.Service
{
    public class StockPromotionnelService : IStockPromotionnelService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        public StockPromotionnelService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;

        }
        public async Task<StockGratuiteDto?> CreateUpdateStockGratuiteAsync(StockGratuiteDto stockDto)
        {

            var entity = await _db.StocksDelegues
                            .OfType<Stock_Gratuite>() 
                            .FirstOrDefaultAsync(s => s.Id_stock == stockDto.Id_stock);

            if (entity == null)
            {
                entity = _mapper.Map<Stock_Gratuite>(stockDto);
                entity.DateCreation = DateTime.UtcNow;
                entity.IsDeleted = false;

                _db.StocksDelegues.Add(entity);
            }
            else
            {
                _mapper.Map(stockDto, entity);
            }

            await _db.SaveChangesAsync();
            return _mapper.Map<StockGratuiteDto>(entity);

        }
        public async Task<StockGratuiteDto?> GetStockGratuiteByIdAsync(int idStock)
        {

            var entity = await _db.StocksDelegues
                            .OfType<Stock_Gratuite>() 
                            .AsNoTracking()
                            .FirstOrDefaultAsync(s =>
                                s.Id_stock == idStock &&
                                !s.IsDeleted);

            if (entity == null)
            {
                return null;
            }
            return _mapper.Map<StockGratuiteDto>(entity);
        }
        public async Task<StockEchantillonDto?> CreateUpdateStockEchantillonAsync(StockEchantillonDto stockDto)
        {

            var entity = await _db.StocksDelegues
                            .OfType<Stock_Echantillon>() 
                            .FirstOrDefaultAsync(s => s.Id_stock == stockDto.Id_stock);

            if (entity == null)
            {
                entity = _mapper.Map<Stock_Echantillon>(stockDto);
                entity.DateCreation = DateTime.UtcNow;
                entity.IsDeleted = false;

                _db.StocksDelegues.Add(entity);
            }
            else
            {
                _mapper.Map(stockDto, entity);
            }

            await _db.SaveChangesAsync();
            return _mapper.Map<StockEchantillonDto>(entity);

        }
        public async Task<StockEchantillonDto?> GetStockEchantillonByIdAsync(int idStock)
        {

            var entity = await _db.StocksDelegues
                            .OfType<Stock_Echantillon>()
                            .AsNoTracking()
                            .FirstOrDefaultAsync(s =>
                                s.Id_stock == idStock &&
                                !s.IsDeleted);
            if (entity == null)
            {
                return null;
            }
            return _mapper.Map<StockEchantillonDto>(entity);
        }

        public async Task<IEnumerable<StockGratuiteDto>> GetAllGratuiteAsync(int page, int size)
        {
            var list = await _db.StocksDelegues
                .OfType<Stock_Gratuite>()
                .AsNoTracking()
                .Where(s => !s.IsDeleted)
                .OrderByDescending(s => s.DateCreation)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();
            return _mapper.Map<IEnumerable<StockGratuiteDto>>(list);
        }

        public async Task<IEnumerable<StockGratuiteDto>> GetGratuiteByDelegueAsync(int idDelegue)
        {
            var list = await _db.StocksDelegues
                .OfType<Stock_Gratuite>()
                .AsNoTracking()
                .Where(s => !s.IsDeleted && s.Id_User_Delegue == idDelegue)
                .OrderByDescending(s => s.DateCreation)
                .ToListAsync();
            return _mapper.Map<IEnumerable<StockGratuiteDto>>(list);
        }

        public async Task<IEnumerable<StockEchantillonDto>> GetAllEchantillonAsync(int page, int size)
        {
            var list = await _db.StocksDelegues
                .OfType<Stock_Echantillon>()
                .AsNoTracking()
                .Where(s => !s.IsDeleted)
                .OrderByDescending(s => s.DateCreation)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();
            return _mapper.Map<IEnumerable<StockEchantillonDto>>(list);
        }

        public async Task<IEnumerable<StockEchantillonDto>> GetEchantillonByDelegueAsync(int idDelegue)
        {
            var list = await _db.StocksDelegues
                .OfType<Stock_Echantillon>()
                .AsNoTracking()
                .Where(s => !s.IsDeleted && s.Id_User_Delegue == idDelegue)
                .OrderByDescending(s => s.DateCreation)
                .ToListAsync();
            return _mapper.Map<IEnumerable<StockEchantillonDto>>(list);
        }

        public async Task<bool> DeleteStockPromotionnelAsync(int idStock)
        {
            var entity = await _db.StocksDelegues.FirstOrDefaultAsync(s => s.Id_stock == idStock);
            if (entity == null || entity.IsDeleted)
            {
                return false;
            }

            entity.IsDeleted = true;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
