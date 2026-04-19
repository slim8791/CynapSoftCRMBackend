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
        }
            // ================================
            // 🔹 REGION
            // ================================

        public async Task<RegionDto?> CreateOrUpdateRegionAsync(RegionDto dto)
        {

            Region region;

            // ➕ Création
            if (dto.Id_Region == 0)
            {
                region = _mapper.Map<Region>(dto);
                _db.Regions.Add(region);
            }
            // ✏️ Mise à jour
            else
            {
                region = await _db.Regions.FirstOrDefaultAsync(r => r.Id_Region == dto.Id_Region);

                if (region == null)
                    return null;

                _mapper.Map(dto, region);
            }

            await _db.SaveChangesAsync();
            return _mapper.Map<RegionDto>(region);

        }

        public async Task<RegionDto?> GetRegionByIdAsync(int idRegion)
        {
            var region = await _db.Regions
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id_Region == idRegion);

            return region == null ? null : _mapper.Map<RegionDto>(region);
        }

        public async Task<IEnumerable<RegionDto>> GetRegionsByDelegueAsync(int idDelegue)
        {
            var list = await _db.Regions
                .AsNoTracking()
                .Where(r => r.Id_User_Delegue == idDelegue)
                .OrderBy(r => r.NomRegion)
                .ToListAsync();

            return _mapper.Map<IEnumerable<RegionDto>>(list);
        }

        public async Task<bool> DeleteRegionAsync(int idRegion)
        {
            var region = await _db.Regions.FirstOrDefaultAsync(r => r.Id_Region == idRegion);
            if (region == null) return false;

            _db.Regions.Remove(region);
            await _db.SaveChangesAsync();
            return true;
        }

        
        public async Task<int> GetNombreRegionsCouvreAsync(int idDelegue)
        {

            return await _db.Regions
                            .CountAsync(r => r.Id_User_Delegue == idDelegue);

        }

    }
}

