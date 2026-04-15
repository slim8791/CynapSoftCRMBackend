using AutoMapper;
using CynapCRM.Services.FieldAPI.Data;
using CynapCRM.Services.FieldAPI.Models;
using CynapCRM.Services.FieldAPI.Models.Dto;
using CynapCRM.Services.FieldAPI.Service.IService;
using Microsoft.EntityFrameworkCore;

namespace CynapCRM.Services.FieldAPI.Service
{
    public class RegionService : IRegionService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public RegionService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
            // ================================
            // 🔹 REGION
            // ================================

        public async Task<RegionDto?> CreateOrUpdateRegionAsync(RegionDto dto)
        {
            var entity = _mapper.Map<Region>(dto);

            var existing = await _db.Regions
                .FirstOrDefaultAsync(r => r.Id_Region == dto.Id_Region);

            if (existing == null)
            {
                _db.Regions.Add(entity);
            }
            else
            {
                _db.Entry(existing).CurrentValues.SetValues(entity);
            }

            await _db.SaveChangesAsync();
            return _mapper.Map<RegionDto>(entity);
        }

        public async Task<IEnumerable<RegionDto>> GetRegionsByDelegueAsync(int idDelegue)
        {
            var list = await _db.Regions
                .AsNoTracking()
                .Where(r => r.Id_User_Delegue == idDelegue)
                .ToListAsync();

            return _mapper.Map<IEnumerable<RegionDto>>(list);
        }

        public async Task<bool> DeleteRegionAsync(int idRegion)
        {
            var entity = await _db.Regions.FindAsync(idRegion);
            if (entity == null) return false;

            _db.Regions.Remove(entity);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignRegionToDelegueAsync(int idRegion, int idDelegue)
        {
            var region = await _db.Regions.FindAsync(idRegion);
            if (region == null) return false;

            region.Id_User_Delegue = idDelegue;
            await _db.SaveChangesAsync();
            return true;
        }
        public async Task<int> GetNombreRegionsCouvreAsync(int idDelegue)
        {
            return await _db.Visites
                .AsNoTracking()
                .Where(v => v.Id_User_Delegue == idDelegue)
                .Select(v => v.Id_Region) // ou IdRegion si tu l’ajoutes
                .Distinct()
                .CountAsync();
        }

    }
}
}
