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
        public async Task<RegionDto?> CreateOrUpdateRegionAsync(RegionDto dto)
        {
            Region region;

            if (dto.Id_Region == 0)
            {
                region = new Region
                {
                    NomRegion = dto.NomRegion,
                    CodePostal = dto.CodePostal,
                    Id_User_Delegue = dto.Id_User_Delegue
                };

                _db.Regions.Add(region);
            }
            else
            {
                region = await _db.Regions
                    .FirstOrDefaultAsync(r => r.Id_Region == dto.Id_Region);

                if (region == null)
                    return null;

                region.NomRegion = dto.NomRegion;
                region.CodePostal = dto.CodePostal;

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

            return await _db.Regions.CountAsync(r => r.Id_User_Delegue == idDelegue);

        }


        public async Task<IEnumerable<RegionDto>> GetAllRegionsAsync()
        {
            var regions = await _db.Regions
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<IEnumerable<RegionDto>>(regions);
        }

    }
}

